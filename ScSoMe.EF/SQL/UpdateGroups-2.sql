UPDATE [scSoMe].[dbo].[Comments] SET group_id = 1 WHERE group_id <> 1 AND group_id <> 12;

DELETE FROM [scSoMe].[dbo].[Groups] WHERE group_id <> 1 AND group_id <> 12;

UPDATE [scSoMe].[dbo].[Groups] SET groupname = 'Events', url = 'events' WHERE group_id = 12;