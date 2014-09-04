SELECT Instance, AVG(Duration) AS AverageDuration, COUNT(*) AS NumberOfOperations
FROM (
	SELECT OperationStart.Instance, (to_unix_timestamp(OperationEnd.Time) - to_unix_timestamp(OperationStart.Time)) AS Duration
	FROM (
		SELECT Time, Instance, OperationId
		FROM Logs2
		WHERE instr(Message, 'Operation started') > 0
	) OperationStart JOIN (
		SELECT Time, Instance, OperationId
		FROM Logs2
		WHERE instr(Message, 'Operation succeeded') > 0
	) OperationEnd ON OperationStart.OperationId = OperationEnd.OperationId
) JoinedLog
GROUP BY Instance
ORDER BY Instance;