USE [aris_public_web]
GO

/****** Object:  UserDefinedFunction [dbo].[udfCountOfLabsUnderRC]    Script Date: 8/4/2016 1:37:12 PM ******/
DROP FUNCTION [dbo].[udfCountOfLabsUnderRC]
GO

/****** Object:  UserDefinedFunction [dbo].[udfCountOfLabsUnderRC]    Script Date: 8/4/2016 1:37:12 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE FUNCTION [dbo].[udfCountOfLabsUnderRC] (@ModeCode int)
RETURNS VARCHAR(50)
AS BEGIN
 declare @CountOfLabsUnderRC int
 declare @ParentAreaModeCode int,   @ParentCityModeCode int, @ParentRC int,   @Lab int ,@location varchar(max)
set @ParentAreaModeCode= cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 1, 2)  as int) set @ParentCityModeCode  = cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 3, 2)  as int)
set @ParentRC = cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 5, 2)  as int)  set @Lab= cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 7, 2)  as int)
 select  @CountOfLabsUnderRC=
				 count (MODECODE_3 )  from  aris_public_web.dbo.REF_MODECODE

where MODECODE_1 =@ParentAreaModeCode and  MODECODE_2 =@ParentCityModeCode  and MODECODE_3=@ParentRC  
and MODECODE_2<>0 
and MODECODE_3<>0 
and modecode_4<>0
and modecode_4<>1
and MODECODE_2_DESC  not like '%Office of Administrator%'
and MODECODE_3_DESC not like '%Location Support Staff%'
and STATUS_CODE='A'

group by MODECODE_1,MODECODE_2,MODECODE_3,MODECODE_3_DESC
if(@CountOfLabsUnderRC is  null)
			    set @CountOfLabsUnderRC='0'

				return @CountOfLabsUnderRC
END
 --select  [dbo].[udfCountOfLabsUnderRC] (20200500)



GO


