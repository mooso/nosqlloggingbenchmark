SELECT Instance, COUNT(*) AS NumberOfErrors
FROM (SELECT Instance, CASE instr(Message, 'Operation failed') WHEN 0 THEN false ELSE true END AS HasErrors
      FROM Logs) ErrorOccurrence
WHERE HasErrors
GROUP BY Instance;