
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 11/25/2014 12:17:12
-- Generated from EDMX file: C:\Users\fernando.oliveira\OneDrive\CMC\POC - Taxas\POC_Taxas\POC.DataBase\TaxModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [POC_TAX];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_DISCOUNT_TAX]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TAX] DROP CONSTRAINT [FK_DISCOUNT_TAX];
GO
IF OBJECT_ID(N'[dbo].[FK_TAX_CHANNEL]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TAX] DROP CONSTRAINT [FK_TAX_CHANNEL];
GO
IF OBJECT_ID(N'[dbo].[FK_TAXCOND_TAX]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TAXCOND] DROP CONSTRAINT [FK_TAXCOND_TAX];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[CHANNEL]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CHANNEL];
GO
IF OBJECT_ID(N'[dbo].[DISCOUNT]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DISCOUNT];
GO
IF OBJECT_ID(N'[dbo].[FORMULAS]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FORMULAS];
GO
IF OBJECT_ID(N'[dbo].[OPERATORS]', 'U') IS NOT NULL
    DROP TABLE [dbo].[OPERATORS];
GO
IF OBJECT_ID(N'[dbo].[ORDERTYPE]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ORDERTYPE];
GO
IF OBJECT_ID(N'[dbo].[TAX]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TAX];
GO
IF OBJECT_ID(N'[dbo].[TAXCOND]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TAXCOND];
GO
IF OBJECT_ID(N'[dbo].[TAXEXCLUSIONS]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TAXEXCLUSIONS];
GO
IF OBJECT_ID(N'[dbo].[UOPG]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UOPG];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'CHANNEL'
CREATE TABLE [dbo].[CHANNEL] (
    [CHANNEL_ID] int IDENTITY(1,1) NOT NULL,
    [CHANNEL_DESCRIPTION] nvarchar(50)  NULL,
    [CHANNEL_CREATEDATE] datetime  NULL,
    [CHANNEL_CODE] varchar(10)  NOT NULL
);
GO

-- Creating table 'DISCOUNT'
CREATE TABLE [dbo].[DISCOUNT] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [VALUE] float  NOT NULL
);
GO

-- Creating table 'FORMULAS'
CREATE TABLE [dbo].[FORMULAS] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [NAME] nvarchar(max)  NOT NULL,
    [FORMULA] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'OPERATORS'
CREATE TABLE [dbo].[OPERATORS] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [OPERATOR] nvarchar(max)  NOT NULL,
    [VALUE] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'ORDERTYPE'
CREATE TABLE [dbo].[ORDERTYPE] (
    [ORDERTYPE_ID] int  NOT NULL,
    [ORDERTYPE_TITLE] nvarchar(100)  NULL,
    [ORDERTYPE_DESCRIPTION] nvarchar(1000)  NULL,
    [ORDERTYPE_XMLFORM] nvarchar(max)  NULL,
    [ORDERTYPE_BUSINESSUNITKEY] varchar(10)  NOT NULL,
    [ORDERTYPE_GDCCID] int  NULL,
    [ORDERTYPE_NEEDSLOGIN] bit  NULL,
    [ORDERTYPE_CREATEDATE] datetime  NULL,
    [ORDERTYPE_PDFFORM_URL] nvarchar(255)  NULL,
    [ORDERTYPE_REDIRECT] nvarchar(500)  NULL,
    [ORDERTYPE_ORDER_SCHEMA] nvarchar(max)  NULL,
    [ORDERTYPE_ORDER_ROOTNAME] nvarchar(50)  NULL
);
GO

-- Creating table 'TAX'
CREATE TABLE [dbo].[TAX] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [ORDERTYPE_ID] int  NOT NULL,
    [DISCOUNT_ID] int  NULL,
    [CHANNEL_ID] int  NOT NULL,
    [NAME] nvarchar(max)  NOT NULL,
    [DESCRIPTION] nvarchar(max)  NOT NULL,
    [START_DATE] datetime  NOT NULL,
    [END_DATE] datetime  NULL
);
GO

-- Creating table 'TAXCOND'
CREATE TABLE [dbo].[TAXCOND] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [TAX_ID] int  NOT NULL,
    [FORM_ITEM_NAME] nvarchar(max)  NOT NULL,
    [FORM_ITEM_VALUE] nvarchar(max)  NOT NULL,
    [FORMULA_ID] int  NULL
);
GO

-- Creating table 'TAXEXCLUSIONS'
CREATE TABLE [dbo].[TAXEXCLUSIONS] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [ORDERTYPE_ID] int  NOT NULL,
    [TAXCOND_ID] int  NOT NULL,
    [FOR_VIEW] bit  NOT NULL
);
GO

-- Creating table 'UOPG'
CREATE TABLE [dbo].[UOPG] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Services] float  NOT NULL,
    [Residence] float  NOT NULL,
    [Industry] float  NOT NULL,
    [Tourism] float  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [CHANNEL_ID] in table 'CHANNEL'
ALTER TABLE [dbo].[CHANNEL]
ADD CONSTRAINT [PK_CHANNEL]
    PRIMARY KEY CLUSTERED ([CHANNEL_ID] ASC);
GO

-- Creating primary key on [ID] in table 'DISCOUNT'
ALTER TABLE [dbo].[DISCOUNT]
ADD CONSTRAINT [PK_DISCOUNT]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'FORMULAS'
ALTER TABLE [dbo].[FORMULAS]
ADD CONSTRAINT [PK_FORMULAS]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'OPERATORS'
ALTER TABLE [dbo].[OPERATORS]
ADD CONSTRAINT [PK_OPERATORS]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ORDERTYPE_ID] in table 'ORDERTYPE'
ALTER TABLE [dbo].[ORDERTYPE]
ADD CONSTRAINT [PK_ORDERTYPE]
    PRIMARY KEY CLUSTERED ([ORDERTYPE_ID] ASC);
GO

-- Creating primary key on [ID] in table 'TAX'
ALTER TABLE [dbo].[TAX]
ADD CONSTRAINT [PK_TAX]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'TAXCOND'
ALTER TABLE [dbo].[TAXCOND]
ADD CONSTRAINT [PK_TAXCOND]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'TAXEXCLUSIONS'
ALTER TABLE [dbo].[TAXEXCLUSIONS]
ADD CONSTRAINT [PK_TAXEXCLUSIONS]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'UOPG'
ALTER TABLE [dbo].[UOPG]
ADD CONSTRAINT [PK_UOPG]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------