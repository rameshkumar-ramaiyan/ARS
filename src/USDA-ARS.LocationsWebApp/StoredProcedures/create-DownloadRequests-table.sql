use [aris_public_web]

/****** Object:  Table [dbo].[DownloadRequests]    Script Date: 7/11/2016 11:46:53 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[DownloadRequests](
	[downreqid] [int] IDENTITY(1,1) NOT NULL,
	[SoftwareId] [varchar](50) NULL,
	[perFname] [varchar](50) NULL,
	[perLname] [varchar](100) NULL,
	[perMname] [varchar](10) NULL,
	[Email] [varchar](100) NULL,
	[Affiliation] [varchar](100) NULL,
	[Purpose] [varchar](50) NULL,
	[Comments] [varchar](1000) NULL,
	[TimeStamp] [datetime] NULL,
	[software_id] [int] NULL,
	[HTTP_REFERER] [varchar](150) NULL,
	[REMOTE_ADDR] [varchar](100) NULL,
	[City] [varchar](150) NULL,
	[State] [varchar](100) NULL,
	[Country] [varchar](150) NULL,
	[Reference] [varchar](100) NULL,
	[Position] [varchar](100) NULL,
	[SpSysEndTime] [datetime] NULL,
 CONSTRAINT [PK_DownloadRequests] PRIMARY KEY CLUSTERED 
(
	[downreqid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[DownloadRequests] ADD  CONSTRAINT [DF_DownloadRequests_DateAndTime]  DEFAULT (getdate()) FOR [TimeStamp]
GO


