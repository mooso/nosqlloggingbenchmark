SELECT Instance, COUNT(*) AS NumberOfErrors
FROM (SELECT Instance, CASE STRPOS(Message, 'Operation failed') WHEN 0 THEN false ELSE true END AS HasErrors
      FROM Logs) AS ErrorOccurrence
WHERE HasErrors
GROUP BY Instance
ORDER BY COUNT(*) DESC;