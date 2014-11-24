
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 11/24/2014 14:17:55
-- Generated from EDMX file: C:\Users\sandra.chanfana\Documents\Projects\Tax_POC\POC.DataBase\TaxModel.edmx
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


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FORMULAS]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FORMULAS];
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
IF OBJECT_ID(N'[dbo].[OPERATORS]', 'U') IS NOT NULL
    DROP TABLE [dbo].[OPERATORS];
GO
IF OBJECT_ID(N'[dbo].[DISCOUNT]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DISCOUNT];
GO
IF OBJECT_ID(N'[dbo].[UOPG]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UOPG];
GO
IF OBJECT_ID(N'[dbo].[TAXEXCLUSIONS]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TAXEXCLUSIONS];
GO
IF OBJECT_ID(N'[dbo].[CHANNEL]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CHANNEL];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'FORMULAS'
CREATE TABLE [dbo].[FORMULAS] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [NAME] nvarchar(max)  NOT NULL,
    [FORMULA] nvarchar(max)  NOT NULL
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
    [DISCOUNT_ID] int  NOT NULL,
    [CHANNEL_ID] int  NOT NULL,
    [NAME] nvarchar(max)  NOT NULL,
    [DESCRIPTION] nvarchar(max)  NOT NULL,
    [START_DATE] datetime  NOT NULL,
    [END_DATE] datetime  NULL,
    [ORDERTYPEORDERTYPE_ID] int  NOT NULL,
    [DISCOUNTID] int  NULL,
    [CHANNELCHANNEL_ID] int  NOT NULL
);
GO

-- Creating table 'TAXCOND'
CREATE TABLE [dbo].[TAXCOND] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [TAX_ID] int  NOT NULL,
    [FORM_ITEM_NAME] nvarchar(max)  NOT NULL,
    [VALUE] nvarchar(max)  NOT NULL,
    [FORMULA_ID] int  NULL,
    [TAXID] int  NOT NULL
);
GO

-- Creating table 'OPERATORS'
CREATE TABLE [dbo].[OPERATORS] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [OPERATOR] nvarchar(max)  NOT NULL,
    [VALUE] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'DISCOUNT'
CREATE TABLE [dbo].[DISCOUNT] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [VALUE] float  NOT NULL
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

-- Creating table 'TAXEXCLUSIONS'
CREATE TABLE [dbo].[TAXEXCLUSIONS] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [ORDERTYPE_ID] int  NOT NULL,
    [TAXCOND_ID] int  NOT NULL,
    [FOR_VIEW] bit  NOT NULL,
    [ORDERTYPEORDERTYPE_ID] int  NOT NULL,
    [TAXCONDID] int  NOT NULL
);
GO

-- Creating table 'CHANNEL'
CREATE TABLE [dbo].[CHANNEL] (
    [CHANNEL_ID] int IDENTITY(1,1) NOT NULL,
    [CHANNEL_DESCRIPTION] nvarchar(50)  NULL,
    [CHANNEL_CREATEDATE] datetime  NULL,
    [CHANNEL_CODE] varchar(10)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [ID] in table 'FORMULAS'
ALTER TABLE [dbo].[FORMULAS]
ADD CONSTRAINT [PK_FORMULAS]
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

-- Creating primary key on [ID] in table 'OPERATORS'
ALTER TABLE [dbo].[OPERATORS]
ADD CONSTRAINT [PK_OPERATORS]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'DISCOUNT'
ALTER TABLE [dbo].[DISCOUNT]
ADD CONSTRAINT [PK_DISCOUNT]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'UOPG'
ALTER TABLE [dbo].[UOPG]
ADD CONSTRAINT [PK_UOPG]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'TAXEXCLUSIONS'
ALTER TABLE [dbo].[TAXEXCLUSIONS]
ADD CONSTRAINT [PK_TAXEXCLUSIONS]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [CHANNEL_ID] in table 'CHANNEL'
ALTER TABLE [dbo].[CHANNEL]
ADD CONSTRAINT [PK_CHANNEL]
    PRIMARY KEY CLUSTERED ([CHANNEL_ID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [ORDERTYPEORDERTYPE_ID] in table 'TAX'
ALTER TABLE [dbo].[TAX]
ADD CONSTRAINT [FK_TAXORDERTYPE]
    FOREIGN KEY ([ORDERTYPEORDERTYPE_ID])
    REFERENCES [dbo].[ORDERTYPE]
        ([ORDERTYPE_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TAXORDERTYPE'
CREATE INDEX [IX_FK_TAXORDERTYPE]
ON [dbo].[TAX]
    ([ORDERTYPEORDERTYPE_ID]);
GO

-- Creating foreign key on [DISCOUNTID] in table 'TAX'
ALTER TABLE [dbo].[TAX]
ADD CONSTRAINT [FK_TAXDISCOUNT]
    FOREIGN KEY ([DISCOUNTID])
    REFERENCES [dbo].[DISCOUNT]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TAXDISCOUNT'
CREATE INDEX [IX_FK_TAXDISCOUNT]
ON [dbo].[TAX]
    ([DISCOUNTID]);
GO

-- Creating foreign key on [TAXID] in table 'TAXCOND'
ALTER TABLE [dbo].[TAXCOND]
ADD CONSTRAINT [FK_TAXTAXCOND]
    FOREIGN KEY ([TAXID])
    REFERENCES [dbo].[TAX]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TAXTAXCOND'
CREATE INDEX [IX_FK_TAXTAXCOND]
ON [dbo].[TAXCOND]
    ([TAXID]);
GO

-- Creating foreign key on [ORDERTYPEORDERTYPE_ID] in table 'TAXEXCLUSIONS'
ALTER TABLE [dbo].[TAXEXCLUSIONS]
ADD CONSTRAINT [FK_ORDERTYPETAXEXCLUSIONS]
    FOREIGN KEY ([ORDERTYPEORDERTYPE_ID])
    REFERENCES [dbo].[ORDERTYPE]
        ([ORDERTYPE_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ORDERTYPETAXEXCLUSIONS'
CREATE INDEX [IX_FK_ORDERTYPETAXEXCLUSIONS]
ON [dbo].[TAXEXCLUSIONS]
    ([ORDERTYPEORDERTYPE_ID]);
GO

-- Creating foreign key on [TAXCONDID] in table 'TAXEXCLUSIONS'
ALTER TABLE [dbo].[TAXEXCLUSIONS]
ADD CONSTRAINT [FK_TAXCONDTAXEXCLUSIONS]
    FOREIGN KEY ([TAXCONDID])
    REFERENCES [dbo].[TAXCOND]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TAXCONDTAXEXCLUSIONS'
CREATE INDEX [IX_FK_TAXCONDTAXEXCLUSIONS]
ON [dbo].[TAXEXCLUSIONS]
    ([TAXCONDID]);
GO

-- Creating foreign key on [CHANNELCHANNEL_ID] in table 'TAX'
ALTER TABLE [dbo].[TAX]
ADD CONSTRAINT [FK_CHANNELTAX]
    FOREIGN KEY ([CHANNELCHANNEL_ID])
    REFERENCES [dbo].[CHANNEL]
        ([CHANNEL_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_CHANNELTAX'
CREATE INDEX [IX_FK_CHANNELTAX]
ON [dbo].[TAX]
    ([CHANNELCHANNEL_ID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------