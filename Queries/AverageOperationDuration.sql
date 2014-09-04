SELECT Instance, AVG(Duration) AS AverageDuration, COUNT(*) AS NumberOfOperations
FROM (
	SELECT OperationStart.Instance, (to_unixtime(OperationEnd.Time) - to_unixtime(OperationStart.Time)) AS Duration
	FROM (
		SELECT Time, Instance, OperationId
		FROM Logs2
		WHERE STRPOS(Message, 'Operation started') > 0
	) OperationStart JOIN (
		SELECT Time, Instance, OperationId
		FROM Logs2
		WHERE STRPOS(Message, 'Operation succeeded') > 0
	) OperationEnd ON OperationStart.OperationId = OperationEnd.OperationId
) JoinedLog
GROUP BY Instance
ORDER BY Instance;