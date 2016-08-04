USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllReassignModeCodesForCityWithSingleChild]    Script Date: 8/4/2016 12:53:06 PM ******/
DROP PROCEDURE [dbo].[uspgetAllReassignModeCodesForCityWithSingleChild]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllReassignModeCodesForCityWithSingleChild]    Script Date: 8/4/2016 12:53:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO









CREATE PROCEDURE [dbo].[uspgetAllReassignModeCodesForCityWithSingleChild]
@ModeCode int
AS

/*following conditions need to be checked 
1.Check if input value(mode code) passed is city,RC or lab.Determine input value(mode code).
2. Check @CountOfLabsUnderRC count and Check @NoofRecordsForRC count. Keep it aside to be used when input value(mode code) passed is    RC or city
3.If input value(mode code) passed is  lab-return input value(mode code) as it is.
4.If input value(mode code) passed is  RC
	4.1.If @CountOfLabsUnderRC count is <>1--return input value(mode code) as it is.
	4.2.If @CountOfLabsUnderRC count is =1 check if lab exists under RC				
		4.2.1.If lab does not exist under RC ---return input value(mode code) as it is.
		4.2.2.If lab exists under RC ---return lab mode code.

5.If input value(mode code) passed is  City
	-----5.1.If @CountOfRCsUnderCity  is <>1 -----------
	-----5.2.If @CountOfRCsUnderCity count is =1 check if lab exists under RC	-----------		
					5.2.1.If lab does not exist under RC ---return RC mode code.
					5.2.2.If lab exists under RC ---return Lab  mode code.
*/


BEGIN--beginning of SP
--declarations--
	declare @DescriptionOfUnit varchar(max),
	@ParentAreaModeCode int,   @ParentCityModeCode int 	, @ParentRCModeCode int,   @ParentLabModeCode int , @SingleChildRCModeCode int,   @SingleChildLabModeCode int 
	set @ParentAreaModeCode = cast( SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 1, 2)  as int)	set @ParentCityModeCode = cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 3, 2)  as int)
	set @ParentRCModeCode = cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 5, 2)  as int)   	set @ParentLabModeCode = cast(SUBSTRING(CAST(@ModeCode AS VARCHAR(50)), 7, 2)  as int)
	Declare @CountOfLabsUnderRC int, 	 @CountOfRCsUnderCity int,	 	 @CountOfLabsUnderSingleRCUnderCity int
/*	1.Check if input value(mode code) passed is city,RC or lab.Determine input value(mode code).*/
set @DescriptionOfUnit=aris_public_web.[dbo].[udfDescribeUnit](@ModeCode)
/*3.If input value(mode code) passed is  lab-return input value(mode code) as it is.*/
	if (@DescriptionOfUnit='Lab')
	begin
			return  cast (@ModeCode as varchar(max))
	end
/*4.If input value(mode code) passed is  lab-return input value(mode code) as it is.*/
	if (@DescriptionOfUnit='RC')
	begin
			
			set @CountOfLabsUnderRC=aris_public_web.[dbo].[udfCountOfLabsUnderRC](@ModeCode)
			-----4.1.If @CountOfLabsUnderRC count is <>1--return input value(mode code) as it is.-----------
			if(@CountOfLabsUnderRC<>1)			
			begin
				return  cast (@ModeCode as varchar(max))
			end
			-----4.2.If @CountOfLabsUnderRC count is =1 check if lab exists under RC	-----------
			else	
			set @SingleChildLabModeCode= aris_public_web.[dbo].[udfGetChildLabModeCodeUnderRC] (@ModeCode)
					
			begin
			   if(@SingleChildLabModeCode=0)
				-----4.2.1.If lab does not exist under RC ---return input value(mode code) as it is.	-----------
				begin
					return  cast (@ModeCode as varchar(max))
				end
				-----4.2.2.If lab exists under RC ---return lab mode code.-----------
				begin
					return  cast (@SingleChildLabModeCode as varchar(max))
				end
			end
	end
/*5.If input value(mode code) passed is  City.*/
	if (@DescriptionOfUnit='City')
	begin
			set @CountOfRCsUnderCity=aris_public_web.[dbo].[udfCountOfRcsUnderCity](@ModeCode)
			-----5.1.If @CountOfRCsUnderCity  is <>1 -----------
			if(@CountOfRCsUnderCity<>1)			
			begin
				return  cast (@ModeCode as varchar(max))
			end
			-----5.2.If @CountOfRCsUnderCity count is =1 check if lab exists under RC	-----------
			else
			begin	
				set @SingleChildRCModeCode	=aris_public_web.[dbo].[udfGetChildRCModeCodeUnderCity](@ModeCode)
				set @CountOfLabsUnderRC=aris_public_web.[dbo].[udfCountOfLabsUnderRC](@SingleChildRCModeCode)
			
				if(@CountOfLabsUnderRC<>1)
				begin
				-----5.2.1.If lab does not exist under RC ---return input value(mode code) as it is.	-----------
					return  cast (@SingleChildRCModeCode as varchar(max))				
				end
				else
				-----5.2.2.If lab exists under RC ---return lab mode code.-----------
				begin
				set @SingleChildLabModeCode= aris_public_web.[dbo].[udfGetChildLabModeCodeUnderRC] (@SingleChildRCModeCode)
					return  cast (@SingleChildLabModeCode as varchar(max))
				end
			end
	end
		
END--end of sp




/*
3.If input value(mode code) passed is  lab-return input value(mode code) as it is.
   ---EXEC	 [dbo].[uspgetAllReassignModeCodesForCityWithSingleChild](80720520)---working

4.If input value(mode code) passed is  RC
	4.1.If @CountOfLabsUnderRC count is <>1--return input value(mode code) as it is.
	---20300500 ---RC with multiple labs 
	4.2.If @CountOfLabsUnderRC count is =1 check if lab exists under RC				
		4.2.1.If lab does not exist under RC ---return input value(mode code) as it is.
		4.2.2.If lab exists under RC ---return lab mode code.
   ---EXEC	 [dbo].[uspgetAllReassignModeCodesForCityWithSingleChild](80640500)
5.If input value(mode code) passed is  City
	5.1.If @CountOfRCsUnderCity  is <>1-return input value(mode code) as it is.
	5.2.If @CountOfLabsUnderRC count is =1 check if lab exists under RC				
		5.2.1.If lab does not exist under RC ---return RC mode code.
		5.2.2.If lab exists under RC ---return Lab  mode code.

		-----RC  and one lab
		--80640000---city with one center and one lab--working
		--20200000--city with one rc and multiple labs--not working

*/



		



GO


