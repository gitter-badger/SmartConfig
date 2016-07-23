/****** Script for SelectTopNRows command from SSMS  ******/


SELECT TOP 1000 
	[Name]
	,[Value]
    ,[Environment]
FROM
	[SmartConfigTest].[dbo].[Setting]


UPDATE [Setting]
	SET [Value] = 'Hallo update!'
	WHERE [Name]='baz' AND [Environment] = 'boz'
IF @@ROWCOUNT = 0 
	INSERT INTO [Setting]([Name], [Value], [Environment])
	VALUES ('baz', 'Hallo insert!', 'boz')


