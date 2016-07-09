if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetBorrowData]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetBorrowData]
GO


SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO
/*
	add:sxb
	add Date:2014-4-29
	description: 借口信息
*/
create  procedure GetBorrowData
@dwguid nvarchar(4000)=null,
@depid nvarchar(4000)=null,
@personid nvarchar(50)=null,
@startdate nvarchar(20)=null,
@enddate nvarchar(20)=null
as
begin
 create table #borrowtemp
 (
	[id] int identity(1,1)primary key, 
   	guid uniqueidentifier null,/*--GUID*/
	docdate nvarchar(20),/*--单据日期*/
	dwname nvarchar(50),/*单位名称*/
	DepartmentName nvarchar(50),
	PersonName nvarchar(50),
	bgcodename nvarchar(50),/*科目名称*/
	docMemo nvarchar(1000),/*摘要*/
	total_wl float not null,/*往来金额*/
	wltypename nvarchar(50),/*往来类型名称*/
	settletypename nvarchar(50),/*结算方式*/
	projectname nvarchar(50),/*项目名称*/
	customername nvarchar(50),/*客户名称*/
	DrawMoneyType nvarchar(10),/*是否 已领款 领款类型*/
	RePamyment nvarchar(10), /*已还款 类型*/	
	RePayData nvarchar(20)/*还款日期*/
				
 )
/*存放借款中的GUID*/	
create table #detailidtemp
(
	[id] int identity(1,1) primary key,
	guid uniqueidentifier null
	
)
declare @sql nvarchar(4000)
declare @sqlWhere nvarchar(4000)
set @sqlWhere=' select guid from wl_MainView  where doctypekey=10 '
/*
单位
*/

if(isnull(@dwguid,'')<>'')
begin

select @dwguid=dbo.f_split(@dwguid,',')
set @sqlWhere=@sqlWhere+' and guid_dw in('+@dwguid+') '
end
/*
部门	
*/
print @depid
if(isnull(@depid,'')<>'')
begin

select @depid=dbo.f_split(@depid,',')

set @sqlWhere=@sqlWhere+' and guid_department in('+@depid+') '
end
/*
人员	
*/
print @personid
if(isnull(@personid,'')<>'')
begin

set @sqlWhere=@sqlWhere+' and guid_person='+@personid+''
end

if( (isnull(@dwguid,'')='') and (isnull(@depid,'')='') and (isnull(@personid,'')='') )
begin
insert into #detailidtemp 
select guid 
from wl_detailView 
where  guid_wl_main in(select guid from wl_MainView  where doctypekey=10)

end

set @sql='
insert into #detailidtemp 
select guid 
from wl_detailView 
where  guid_wl_main in('+@sqlWhere+')'


exec (@sql)

declare @idIndex int
declare @GUID uniqueidentifier
declare
	@docdate nvarchar(20),/*--单据日期*/
	@dwname nvarchar(50),/*单位名称*/
	@DepartmentName nvarchar(50),
	@PersonName nvarchar(50),
	@bgcodename nvarchar(50),/*科目名称*/
	@docMemo nvarchar(1000),/*摘要*/
	@total_wl float,/*往来金额*/
	@wltypename nvarchar(50),/*往来类型名称*/
	@settletypename nvarchar(50),/*结算方式*/
	@projectname nvarchar(50),/*项目名称*/
	@customername nvarchar(50),/*客户名称*/
	@DrawMoneyType nvarchar(10),/*是否 已领款 领款类型*/
	@RePamyment nvarchar(10), /*已还款 类型*/	
	@RePayData nvarchar(20)

declare @class_wl_detail int /*往来ClassID*/
select  @class_wl_detail=classid from ss_class where tablename='wl_detail'
declare @class_cn_detail int /*出纳Classid*/
select  @class_cn_detail=classid from ss_class where tablename='cn_detail'


select @idIndex=count(*) from #detailidtemp
declare @i int 
set @i=1
while(@i<@idIndex)
begin
 select @GUID=GUID from #detailidtemp where [id]=@i
 select 	
	@docdate=convert(varchar(10),m.docdate,120),
	@dwname=m.dwname,
	@DepartmentName=m.DepartmentName,
	@PersonName=m.PersonName,
	@bgcodename=d.bgcodename,	
	@docMemo=m.docMemo,
	@total_wl=d.total_wl,/*往来金额*/
	@wltypename=d.wltypename,/*往来类型名称*/
	@settletypename=d.settletypename,/*结算方式*/
	@projectname=d.projectname,/*项目名称*/
	@customername=d.customername/*客户名称*/
	
from wl_detailView d
left join  wl_MainView m on d.guid_wl_main=m.guid
where d.guid=@GUID
/*
已经完成核销处理并生成出纳付款单的借款单状态为“已领款”
@DrawMoneyType=DrawMoneyType,是否 已领款 领款类型
*/
set @DrawMoneyType='未领款'
declare @lkCount int /*领款个数*/

select @lkCount=count(*) from hx_detail where classid_detail=@class_wl_detail and guid_detail=@GUID and isdc=1
if (@lkCount>0)
set  @DrawMoneyType='已领款'

/*
@RePamyment=RePamyment  已还款 类型	
*/
set @RePamyment='未还款'
set @RePayData=''
declare @hkCount int /*还款*/
declare @hx_m_guid nvarchar(50) /*核销主表信息*/
select @hkCount=count(*) from hx_detail where classid_detail=@class_wl_detail and guid_detail=@GUID and isdc=0
if(@hkCount>0)
begin
set @RePamyment='已还款'
select  @RePayData=convert(varchar(10),docdate,120) from hx_main where guid in(select guid_hx_main from hx_detail where classid_detail=@class_wl_detail and guid_detail=@GUID and isdc=0)
end
/*添加数据*/
insert into #borrowtemp(
	guid ,/*--GUID*/
	docdate,/*--单据日期*/
	dwname ,/*单位名称*/
	DepartmentName,
	PersonName,
	bgcodename ,/*科目名称*/
	docMemo ,/*摘要*/
	total_wl ,/*往来金额*/
	wltypename ,/*往来类型名称*/
	settletypename ,/*结算方式*/
	projectname ,/*项目名称*/
	customername ,/*客户名称*/
	DrawMoneyType,/*是否 已领款 领款类型*/
	RePamyment, /*是否还款*/
	RePayData
)
values
(
	@guid,
	@docdate,
	@dwname,	
	@DepartmentName,
	@PersonName,
	@bgcodename,	
	@docMemo,
	@total_wl,/*往来金额*/
	@wltypename,/*往来类型名称*/
	@settletypename,/*结算方式*/
	@projectname,/*项目名称*/
	@customername,/*客户名称*/
	@DrawMoneyType,
	@RePamyment,
	@RePayData
)
set @i=@i+1
end

select guid ,/*--GUID*/
	DocDate,/*--单据日期*/
	dwname ,/*单位名称*/
	DepartmentName,
	PersonName,
	isnull(bgcodename,'') BGCodeName  ,/*科目名称*/
	isnull(DocMemo,'') DocMemo ,/*摘要*/
	total_wl as Total,/*往来金额*/
	isnull(wltypename,'')WLTypeName ,/*往来类型名称*/
	SettleTypeName ,/*结算方式*/
	isnull(ProjectName,'') ProjectName ,/*项目名称*/
	isnull(CustomerName,'') CustomerName ,/*客户名称*/
	DrawMoneyType,/*是否 已领款 领款类型*/
	RePamyment, /*是否还款*/
	isnull(RePayData,'') as RePayDate  from #borrowtemp

drop table #borrowtemp
drop table #detailidtemp

end
