using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggingApp
{
	class BlobLogger
	{
		private readonly CloudBlockBlob _logBlob;
		private readonly BlockingCollection<BlockContent> _newBlocksQueue = new BlockingCollection<BlockContent>(200);
		private byte[] _currentBlockContent = new byte[MaxBlockSize];
		private int _currentBlockContentOffset;
		private object _currentBlockContentLock = new object();
		private const int MaxBlockSize = 4 * 1024 * 1024;

		public BlobLogger(CloudBlockBlob logBlob)
		{
			_logBlob = logBlob;
			List<string> initialBlockList;
			if (_logBlob.Exists())
			{
				initialBlockList = _logBlob.DownloadBlockList(BlockListingFilter.Committed).Select(b => b.Name).ToList();
			}
			else
			{
				initialBlockList = new List<string>();
				_logBlob.PutBlockList(initialBlockList);
			}
			Task.Factory.StartNew(() => AddNewBlocks(initialBlockList));
		}

		private sealed class BlockContent
		{
			private readonly byte[] _content;
			private readonly int _length;

			public BlockContent(byte[] content, int length)
			{
				_content = content;
				_length = length;
			}

			public Stream CreateStream()
			{
				return new MemoryStream(_content, 0, _length);
			}
		}

		private void AddNewBlocks(List<string> currentBlockList)
		{
			foreach (var newBlock in _newBlocksQueue.GetConsumingEnumerable())
			{
				var blockId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
				using (var dataStream = newBlock.CreateStream())
				{
					_logBlob.PutBlock(blockId, dataStream, null);
				}
				currentBlockList.Add(blockId);
				_logBlob.PutBlockList(currentBlockList);
			}
		}

		private void AddNewContentToCurrentBlock(byte[] newContent)
		{
			lock (_currentBlockContentLock)
			{
				if ((_currentBlockContentOffset + newContent.Length) > MaxBlockSize)
				{
					_newBlocksQueue.Add(new BlockContent(_currentBlockContent, _currentBlockContentOffset));
					_currentBlockContent = new byte[MaxBlockSize];
					_currentBlockContentOffset = 0;
				}
				Buffer.BlockCopy(newContent, 0, _currentBlockContent, _currentBlockContentOffset, newContent.Length);
				_currentBlockContentOffset += newContent.Length;
			}
		}

		public void TraceMessage(Guid operationId, string message)
		{
			var fullMessage = String.Format(CultureInfo.InvariantCulture,
				"{0:yyyy-MM-dd HH:mm:ss.fffffff},{1},{2},\"{3}\"\n", DateTime.Now,
				RoleEnvironment.CurrentRoleInstance.Id,
				operationId,
				message.Replace("\"", "\"\""));
			var newContent = Encoding.UTF8.GetBytes(fullMessage);
			AddNewContentToCurrentBlock(newContent);
		}
	}
}
