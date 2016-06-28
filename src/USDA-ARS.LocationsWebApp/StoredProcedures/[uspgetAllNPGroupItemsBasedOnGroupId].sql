USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllNPGroupItemsBasedOnGroupId]    Script Date: 4/11/2016 7:06:54 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO








CREATE PROCEDURE [dbo].[uspgetAllNPGroupItemsBasedOnGroupId]
@GroupId int 
AS
 
BEGIN
	
select * from sitepublisherii.dbo.NPrograms where NPGroup_ID=@GroupId order by npgroup_id,NPNumber
end

 -- all pages for NP 107
	--0.select * from sitepublisherii.dbo.NPGroups order by NPGroupID
	--1.select * from sitepublisherii.dbo.NPrograms  where npgroup_id=1 order by npgroup_id,NPNumber	
	
GO


