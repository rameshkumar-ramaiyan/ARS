USE [aris_public_web]
GO

/****** Object:  UserDefinedFunction [dbo].[udfCountOfRcsUnderCity]    Script Date: 8/4/2016 12:53:48 PM ******/
DROP FUNCTION [dbo].[udfCountOfRcsUnderCity]
GO

/****** Object:  UserDefinedFunction [dbo].[udfCountOfRcsUnderCity]    Script Date: 8/4/2016 12:53:48 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE FUNCTION [dbo].[udfCountOfRcsUnderCity] (@ModeCode int)
RETURNS VARCHAR(50)
AS BEGIN
 declare @CountOfRcsUnderCity int
 declare @ParentAreaModeCode int,   @ParentCityModeCode int, @ParentRC int,   @Lab int ,@location varchar(max)
set @ParentAreaModeCode= cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 1, 2)  as int) set @ParentCityModeCode  = cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 3, 2)  as int)
set @ParentRC = cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 5, 2)  as int)  set @Lab= cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 7, 2)  as int)
 select  @CountOfRcsUnderCity=
				count(*) from 
				( select MODECODE_1,MODECODE_2,MODECODE_3,MODECODE_3_DESC from
				aris_public_web.dbo.REF_MODECODE 
			where 
				MODECODE_1=@ParentAreaModeCode 
and MODECODE_2=@ParentCityModeCode and MODECODE_2 <> '1'
and MODECODE_3 is not null and MODECODE_3<>0 and MODECODE_3<>1  and MODECODE_3 <> '2'
and MODECODE_4=0 




AND STATUS_CODE = 'A' --status code active                  
	and  STATE_CODE is not null
	
				group by MODECODE_1,MODECODE_2,MODECODE_3,MODECODE_3_DESC
				having count(*)=1
				
				) as cnt
				if(@CountOfRcsUnderCity is  null)
			    set @CountOfRcsUnderCity='0'
				return @CountOfRcsUnderCity
END


--select dbo.[udfCountOfRcsUnderCity](20200000)
GO


