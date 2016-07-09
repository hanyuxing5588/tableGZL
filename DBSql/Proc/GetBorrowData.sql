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
	description: �����Ϣ
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
	docdate nvarchar(20),/*--��������*/
	dwname nvarchar(50),/*��λ����*/
	DepartmentName nvarchar(50),
	PersonName nvarchar(50),
	bgcodename nvarchar(50),/*��Ŀ����*/
	docMemo nvarchar(1000),/*ժҪ*/
	total_wl float not null,/*�������*/
	wltypename nvarchar(50),/*������������*/
	settletypename nvarchar(50),/*���㷽ʽ*/
	projectname nvarchar(50),/*��Ŀ����*/
	customername nvarchar(50),/*�ͻ�����*/
	DrawMoneyType nvarchar(10),/*�Ƿ� ����� �������*/
	RePamyment nvarchar(10), /*�ѻ��� ����*/	
	RePayData nvarchar(20)/*��������*/
				
 )
/*��Ž���е�GUID*/	
create table #detailidtemp
(
	[id] int identity(1,1) primary key,
	guid uniqueidentifier null
	
)
declare @sql nvarchar(4000)
declare @sqlWhere nvarchar(4000)
set @sqlWhere=' select guid from wl_MainView  where doctypekey=10 '
/*
��λ
*/

if(isnull(@dwguid,'')<>'')
begin

select @dwguid=dbo.f_split(@dwguid,',')
set @sqlWhere=@sqlWhere+' and guid_dw in('+@dwguid+') '
end
/*
����	
*/
print @depid
if(isnull(@depid,'')<>'')
begin

select @depid=dbo.f_split(@depid,',')

set @sqlWhere=@sqlWhere+' and guid_department in('+@depid+') '
end
/*
��Ա	
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
	@docdate nvarchar(20),/*--��������*/
	@dwname nvarchar(50),/*��λ����*/
	@DepartmentName nvarchar(50),
	@PersonName nvarchar(50),
	@bgcodename nvarchar(50),/*��Ŀ����*/
	@docMemo nvarchar(1000),/*ժҪ*/
	@total_wl float,/*�������*/
	@wltypename nvarchar(50),/*������������*/
	@settletypename nvarchar(50),/*���㷽ʽ*/
	@projectname nvarchar(50),/*��Ŀ����*/
	@customername nvarchar(50),/*�ͻ�����*/
	@DrawMoneyType nvarchar(10),/*�Ƿ� ����� �������*/
	@RePamyment nvarchar(10), /*�ѻ��� ����*/	
	@RePayData nvarchar(20)

declare @class_wl_detail int /*����ClassID*/
select  @class_wl_detail=classid from ss_class where tablename='wl_detail'
declare @class_cn_detail int /*����Classid*/
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
	@total_wl=d.total_wl,/*�������*/
	@wltypename=d.wltypename,/*������������*/
	@settletypename=d.settletypename,/*���㷽ʽ*/
	@projectname=d.projectname,/*��Ŀ����*/
	@customername=d.customername/*�ͻ�����*/
	
from wl_detailView d
left join  wl_MainView m on d.guid_wl_main=m.guid
where d.guid=@GUID
/*
�Ѿ���ɺ����������ɳ��ɸ���Ľ�״̬Ϊ������
@DrawMoneyType=DrawMoneyType,�Ƿ� ����� �������
*/
set @DrawMoneyType='δ���'
declare @lkCount int /*������*/

select @lkCount=count(*) from hx_detail where classid_detail=@class_wl_detail and guid_detail=@GUID and isdc=1
if (@lkCount>0)
set  @DrawMoneyType='�����'

/*
@RePamyment=RePamyment  �ѻ��� ����	
*/
set @RePamyment='δ����'
set @RePayData=''
declare @hkCount int /*����*/
declare @hx_m_guid nvarchar(50) /*����������Ϣ*/
select @hkCount=count(*) from hx_detail where classid_detail=@class_wl_detail and guid_detail=@GUID and isdc=0
if(@hkCount>0)
begin
set @RePamyment='�ѻ���'
select  @RePayData=convert(varchar(10),docdate,120) from hx_main where guid in(select guid_hx_main from hx_detail where classid_detail=@class_wl_detail and guid_detail=@GUID and isdc=0)
end
/*�������*/
insert into #borrowtemp(
	guid ,/*--GUID*/
	docdate,/*--��������*/
	dwname ,/*��λ����*/
	DepartmentName,
	PersonName,
	bgcodename ,/*��Ŀ����*/
	docMemo ,/*ժҪ*/
	total_wl ,/*�������*/
	wltypename ,/*������������*/
	settletypename ,/*���㷽ʽ*/
	projectname ,/*��Ŀ����*/
	customername ,/*�ͻ�����*/
	DrawMoneyType,/*�Ƿ� ����� �������*/
	RePamyment, /*�Ƿ񻹿�*/
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
	@total_wl,/*�������*/
	@wltypename,/*������������*/
	@settletypename,/*���㷽ʽ*/
	@projectname,/*��Ŀ����*/
	@customername,/*�ͻ�����*/
	@DrawMoneyType,
	@RePamyment,
	@RePayData
)
set @i=@i+1
end

select guid ,/*--GUID*/
	DocDate,/*--��������*/
	dwname ,/*��λ����*/
	DepartmentName,
	PersonName,
	isnull(bgcodename,'') BGCodeName  ,/*��Ŀ����*/
	isnull(DocMemo,'') DocMemo ,/*ժҪ*/
	total_wl as Total,/*�������*/
	isnull(wltypename,'')WLTypeName ,/*������������*/
	SettleTypeName ,/*���㷽ʽ*/
	isnull(ProjectName,'') ProjectName ,/*��Ŀ����*/
	isnull(CustomerName,'') CustomerName ,/*�ͻ�����*/
	DrawMoneyType,/*�Ƿ� ����� �������*/
	RePamyment, /*�Ƿ񻹿�*/
	isnull(RePayData,'') as RePayDate  from #borrowtemp

drop table #borrowtemp
drop table #detailidtemp

end
