SELECT Instance, OperationStatus, COUNT(*) AS Number
FROM (SELECT Instance,
		CASE WHEN instr(Message, 'Operation failed') > 0 THEN 'Error'
			 WHEN instr(Message, 'Operation succeeded') > 0 THEN 'Success'
			 ELSE 'None'
		END AS OperationStatus
      FROM Logs2) OperationStatus
WHERE OperationStatus != 'None'
GROUP BY Instance, OperationStatus
ORDER BY Instance, OperationStatus;