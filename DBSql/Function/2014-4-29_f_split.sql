
if exists(select * from dbo.sysobjects where id=object_id(N'[dbo].[f_split]') and xtype in(N'FN',N'IF',N'TF'))
drop function [dbo].[f_split]
GO


SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO
/*
add:sxb
add Date:2014-4-29
Description: ²ð·Ö×Ö·û´®
*/
create function f_split(@str nvarchar(4000),@sign varchar(2))
returns nvarchar(4000)
as
begin
declare @strValue nvarchar(4000)
declare @location int
set @strValue=''
set @str=ltrim(rtrim(@str))
set @location=charindex(@sign,@str)

while (@location<>0)
begin	
	
	set @strValue=@strValue+''''+substring(@str,1,@location-1)+''''+','
	set @str=stuff(@str,1,@location,'')
	set @location=charindex(@sign,@str)
end
	set @strValue=@strValue+''''+@str+''''
return @strValue
end