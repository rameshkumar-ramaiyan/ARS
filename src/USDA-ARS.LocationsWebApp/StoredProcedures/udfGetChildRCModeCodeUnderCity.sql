USE [aris_public_web]
GO

/****** Object:  UserDefinedFunction [dbo].[udfGetChildRCModeCodeUnderCity]    Script Date: 8/4/2016 12:54:09 PM ******/
DROP FUNCTION [dbo].[udfGetChildRCModeCodeUnderCity]
GO

/****** Object:  UserDefinedFunction [dbo].[udfGetChildRCModeCodeUnderCity]    Script Date: 8/4/2016 12:54:09 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION [dbo].[udfGetChildRCModeCodeUnderCity] (@ModeCode int)
RETURNS int
AS BEGIN
 declare @ParentAreaModeCode int,   @ParentCityModeCode int, @ParentRC int,   @Lab int ,@location varchar(max)
set @ParentAreaModeCode= cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 1, 2)  as int)
set @ParentCityModeCode  = cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 3, 2)  as int)
set @ParentRC = cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 5, 2)  as int)
set @Lab= cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 7, 2)  as int)
declare @ret varchar(max)



select @ret=  

cast (MODECODE_1 as varchar(2))+cast (MODECODE_2 as varchar(2))+
CASE 
	WHEN MODECODE_3  >=10 THEN cast (MODECODE_3 as varchar(2))
	ELSE '0'+ cast (MODECODE_3 as varchar(2)) 
	END 


+'00'


from aris_public_web.dbo.REF_MODECODE 
where 
MODECODE_1=@ParentAreaModeCode 
and MODECODE_2=@ParentCityModeCode and MODECODE_2 <> '1'
and MODECODE_3 is not null and MODECODE_3<>0 and MODECODE_3<>1  and MODECODE_3 <> '2'
and MODECODE_4=0 




AND STATUS_CODE = 'A' --status code active                  
	and  STATE_CODE is not null
	order by MODECODE_1,MODECODE_2,MODECODE_3,MODECODE_3_DESC
 if(@ret is  null)
  set @ret='0'
 
 
 return cast (@ret as int)
END


GO


