CREATE EXTERNAL TABLE Logs2
(Time timestamp, Instance string, OperationId string, Message string)
ROW FORMAT DELIMITED FIELDS TERMINATED BY ','
LOCATION 'wasb://bloblogs2@moswest.blob.core.windows.net/logs'
