CREATE EXTERNAL TABLE Logs
(Time string, Instance string, Message string)
ROW FORMAT DELIMITED FIELDS TERMINATED BY ','
LOCATION 'wasb://bloblogs@moswest.blob.core.windows.net/logs'
