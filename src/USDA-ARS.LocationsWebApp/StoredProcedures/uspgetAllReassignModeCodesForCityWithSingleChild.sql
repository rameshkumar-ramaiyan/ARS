USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllReassignModeCodesForCityWithSingleChild]    Script Date: 8/3/2016 10:00:37 AM ******/
DROP PROCEDURE [dbo].[uspgetAllReassignModeCodesForCityWithSingleChild]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllReassignModeCodesForCityWithSingleChild]    Script Date: 8/3/2016 10:00:37 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO








CREATE PROCEDURE [dbo].[uspgetAllReassignModeCodesForCityWithSingleChild]
@ModeCode int
AS

BEGIN
 declare @ParentAreaModeCode int,   @ParentCityModeCode int 
		
----row count>1
	set @ParentAreaModeCode = cast( SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 1, 2)  as int)
	set @ParentCityModeCode = cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 3, 2)  as int)
	Declare @VariableName int

	select  @VariableName=
  count(*) from 
  ( select MODECODE_1,MODECODE_2,MODECODE_3,MODECODE_3_DESC from
  aris_public_webNew.dbo.REF_MODECODE 
where 
MODECODE_1 in (@ParentAreaModeCode )
and MODECODE_2 in(@ParentCityModeCode) and MODECODE_2 <> '1'
and MODECODE_3 is not null and MODECODE_3<>0 and MODECODE_3<>1  and MODECODE_3 <> '2'
and MODECODE_4=0 
AND STATUS_CODE = 'A' --status code active                  
and  STATE_CODE is not null
group by MODECODE_1,MODECODE_2,MODECODE_3,MODECODE_3_DESC
having count(*)=1
) as cnt

 
 if
(@VariableName=1)

begin


select
cast (MODECODE_1 as varchar(2))+'-'+cast (MODECODE_2 as varchar(2))+'-'+
CASE 
WHEN MODECODE_3  >=10 THEN cast (MODECODE_3 as varchar(2))
ELSE '0'+ cast (MODECODE_3 as varchar(2)) 
END 

+'-00'as 'Mode Code'

,MODECODE_3_DESC as 'Research Unit' 
--,MODECODE_4 as 'Lab Mode Code',MODECODE_4_DESC as 'Lab'
from aris_public_webNew.dbo.REF_MODECODE 
where 
MODECODE_1=@ParentAreaModeCode 
and MODECODE_2=@ParentCityModeCode and MODECODE_2 <> '1'
and MODECODE_3 is not null and MODECODE_3<>0 and MODECODE_3<>1  and MODECODE_3 <> '2'
and MODECODE_4=0 
AND STATUS_CODE = 'A' --status code active                  
and  STATE_CODE is not null
group by MODECODE_1,MODECODE_2,MODECODE_3,MODECODE_3_DESC
order by MODECODE_1,MODECODE_2,MODECODE_3,MODECODE_3_DESC
	 
end

	else 
	begin
	 return @ModeCode
	 end
		
END





GO


