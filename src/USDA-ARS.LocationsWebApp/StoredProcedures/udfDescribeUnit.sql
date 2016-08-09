USE [aris_public_webNew]
GO
/****** Object:  UserDefinedFunction [dbo].[udfDescribeUnit]    Script Date: 8/9/2016 1:26:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER FUNCTION [dbo].[udfDescribeUnit] (@ModeCode int)
RETURNS VARCHAR(50)
AS BEGIN
 declare @ParentAreaModeCode int,   @ParentCityModeCode int, @ParentRC int,   @Lab int ,@location varchar(max)
set @ParentAreaModeCode= cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 1, 2)  as int)
set @ParentCityModeCode  = cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 3, 2)  as int)
set @ParentRC = cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 5, 2)  as int)
set @Lab= cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 7, 2)  as int)
 set @location='Empty'
	If(@Lab<>0)
	set @location='Lab'
	else	
		if(@LAb=0 and @ParentRC<>0)
		set @location='RC'
		else if(@LAb=0 and @ParentRC=0 )
			if(@ParentCityModeCode<>0)
			set @location='City'
			else
			set @location='Area'

			return @location
END

