
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 12/09/2014 11:16:55
-- Generated from EDMX file: C:\Users\fernando.oliveira\OneDrive\CMC\POC - Taxas\POC_Taxas\POC.DataBase\TaxModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [POC_TAX];
GO
IF SCHEMA_ID(N'TAX') IS NULL EXECUTE(N'CREATE SCHEMA [TAX]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[TAX].[FK_TAX_CHANNEL]', 'F') IS NOT NULL
    ALTER TABLE [TAX].[TAXES] DROP CONSTRAINT [FK_TAX_CHANNEL];
GO
IF OBJECT_ID(N'[TAX].[FK_TAX_DISCOUNT]', 'F') IS NOT NULL
    ALTER TABLE [TAX].[TAXES] DROP CONSTRAINT [FK_TAX_DISCOUNT];
GO
IF OBJECT_ID(N'[TAX].[FK_TAX_FORMULAS]', 'F') IS NOT NULL
    ALTER TABLE [TAX].[TAXES] DROP CONSTRAINT [FK_TAX_FORMULAS];
GO
IF OBJECT_ID(N'[TAX].[FK_TAXCONDS_FORMULAS]', 'F') IS NOT NULL
    ALTER TABLE [TAX].[TAXCONDS] DROP CONSTRAINT [FK_TAXCONDS_FORMULAS];
GO
IF OBJECT_ID(N'[TAX].[FK_OPERATORS_VALUETYPES]', 'F') IS NOT NULL
    ALTER TABLE [TAX].[OPERATORS] DROP CONSTRAINT [FK_OPERATORS_VALUETYPES];
GO
IF OBJECT_ID(N'[TAX].[FK_TAX_ORDERTYPE]', 'F') IS NOT NULL
    ALTER TABLE [TAX].[TAXES] DROP CONSTRAINT [FK_TAX_ORDERTYPE];
GO
IF OBJECT_ID(N'[TAX].[FK_TAXCOND_TAX]', 'F') IS NOT NULL
    ALTER TABLE [TAX].[TAXCONDS] DROP CONSTRAINT [FK_TAXCOND_TAX];
GO
IF OBJECT_ID(N'[TAX].[FK_TAXEXCLUSIONS_TAXCOND]', 'F') IS NOT NULL
    ALTER TABLE [TAX].[TAXEXCLUSIONS] DROP CONSTRAINT [FK_TAXEXCLUSIONS_TAXCOND];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[TAX].[CHANNELS]', 'U') IS NOT NULL
    DROP TABLE [TAX].[CHANNELS];
GO
IF OBJECT_ID(N'[TAX].[DISCOUNTS]', 'U') IS NOT NULL
    DROP TABLE [TAX].[DISCOUNTS];
GO
IF OBJECT_ID(N'[TAX].[FORMULAS]', 'U') IS NOT NULL
    DROP TABLE [TAX].[FORMULAS];
GO
IF OBJECT_ID(N'[TAX].[OPERATORS]', 'U') IS NOT NULL
    DROP TABLE [TAX].[OPERATORS];
GO
IF OBJECT_ID(N'[TAX].[ORDERTYPES]', 'U') IS NOT NULL
    DROP TABLE [TAX].[ORDERTYPES];
GO
IF OBJECT_ID(N'[TAX].[TAXCONDS]', 'U') IS NOT NULL
    DROP TABLE [TAX].[TAXCONDS];
GO
IF OBJECT_ID(N'[TAX].[TAXES]', 'U') IS NOT NULL
    DROP TABLE [TAX].[TAXES];
GO
IF OBJECT_ID(N'[TAX].[TAXEXCLUSIONS]', 'U') IS NOT NULL
    DROP TABLE [TAX].[TAXEXCLUSIONS];
GO
IF OBJECT_ID(N'[TAX].[UOPG]', 'U') IS NOT NULL
    DROP TABLE [TAX].[UOPG];
GO
IF OBJECT_ID(N'[TAX].[VALUETYPES]', 'U') IS NOT NULL
    DROP TABLE [TAX].[VALUETYPES];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'CHANNELS'
CREATE TABLE [TAX].[CHANNELS] (
    [CHANNEL_ID] int IDENTITY(1,1) NOT NULL,
    [CHANNEL_DESCRIPTION] nvarchar(50)  NULL,
    [CHANNEL_CREATEDATE] datetime  NULL,
    [CHANNEL_CODE] varchar(10)  NOT NULL
);
GO

-- Creating table 'DISCOUNTS'
CREATE TABLE [TAX].[DISCOUNTS] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [VALUE] float  NOT NULL
);
GO

-- Creating table 'FORMULAS'
CREATE TABLE [TAX].[FORMULAS] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [NAME] nvarchar(max)  NOT NULL,
    [FORMULA] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'OPERATORS'
CREATE TABLE [TAX].[OPERATORS] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [VALUETYPE_ID] int  NOT NULL,
    [OPERATOR] nvarchar(max)  NOT NULL,
    [VALUE] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'ORDERTYPES'
CREATE TABLE [TAX].[ORDERTYPES] (
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

-- Creating table 'TAXCONDS'
CREATE TABLE [TAX].[TAXCONDS] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [TAX_ID] int  NOT NULL,
    [FORMULA_ID] int  NULL,
    [FORM_ITEM_NAME] nvarchar(max)  NOT NULL,
    [FORM_ITEM_VALUE] nvarchar(max)  NOT NULL,
    [VALUE_IS_EQUAL] bit  NOT NULL
);
GO

-- Creating table 'TAXES'
CREATE TABLE [TAX].[TAXES] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [ORDERTYPE_ID] int  NOT NULL,
    [DISCOUNT_ID] int  NULL,
    [CHANNEL_ID] int  NOT NULL,
    [FORMULA_ID] int  NULL,
    [NAME] nvarchar(max)  NOT NULL,
    [DESCRIPTION] nvarchar(max)  NOT NULL,
    [START_DATE] datetime  NOT NULL,
    [END_DATE] datetime  NULL,
    [VALUE] decimal(18,0)  NULL,
    [SIMULATE] bit  NOT NULL
);
GO

-- Creating table 'TAXEXCLUSIONS'
CREATE TABLE [TAX].[TAXEXCLUSIONS] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [TAXCOND_ID] int  NULL
);
GO

-- Creating table 'UOPG'
CREATE TABLE [TAX].[UOPG] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Services] float  NOT NULL,
    [Residence] float  NOT NULL,
    [Industry] float  NOT NULL,
    [Tourism] float  NOT NULL
);
GO

-- Creating table 'VALUETYPES'
CREATE TABLE [TAX].[VALUETYPES] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [VALUETYPE] varchar(255)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [CHANNEL_ID] in table 'CHANNELS'
ALTER TABLE [TAX].[CHANNELS]
ADD CONSTRAINT [PK_CHANNELS]
    PRIMARY KEY CLUSTERED ([CHANNEL_ID] ASC);
GO

-- Creating primary key on [ID] in table 'DISCOUNTS'
ALTER TABLE [TAX].[DISCOUNTS]
ADD CONSTRAINT [PK_DISCOUNTS]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'FORMULAS'
ALTER TABLE [TAX].[FORMULAS]
ADD CONSTRAINT [PK_FORMULAS]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'OPERATORS'
ALTER TABLE [TAX].[OPERATORS]
ADD CONSTRAINT [PK_OPERATORS]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ORDERTYPE_ID] in table 'ORDERTYPES'
ALTER TABLE [TAX].[ORDERTYPES]
ADD CONSTRAINT [PK_ORDERTYPES]
    PRIMARY KEY CLUSTERED ([ORDERTYPE_ID] ASC);
GO

-- Creating primary key on [ID] in table 'TAXCONDS'
ALTER TABLE [TAX].[TAXCONDS]
ADD CONSTRAINT [PK_TAXCONDS]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'TAXES'
ALTER TABLE [TAX].[TAXES]
ADD CONSTRAINT [PK_TAXES]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'TAXEXCLUSIONS'
ALTER TABLE [TAX].[TAXEXCLUSIONS]
ADD CONSTRAINT [PK_TAXEXCLUSIONS]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'UOPG'
ALTER TABLE [TAX].[UOPG]
ADD CONSTRAINT [PK_UOPG]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'VALUETYPES'
ALTER TABLE [TAX].[VALUETYPES]
ADD CONSTRAINT [PK_VALUETYPES]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [CHANNEL_ID] in table 'TAXES'
ALTER TABLE [TAX].[TAXES]
ADD CONSTRAINT [FK_TAX_CHANNEL]
    FOREIGN KEY ([CHANNEL_ID])
    REFERENCES [TAX].[CHANNELS]
        ([CHANNEL_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TAX_CHANNEL'
CREATE INDEX [IX_FK_TAX_CHANNEL]
ON [TAX].[TAXES]
    ([CHANNEL_ID]);
GO

-- Creating foreign key on [DISCOUNT_ID] in table 'TAXES'
ALTER TABLE [TAX].[TAXES]
ADD CONSTRAINT [FK_TAX_DISCOUNT]
    FOREIGN KEY ([DISCOUNT_ID])
    REFERENCES [TAX].[DISCOUNTS]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TAX_DISCOUNT'
CREATE INDEX [IX_FK_TAX_DISCOUNT]
ON [TAX].[TAXES]
    ([DISCOUNT_ID]);
GO

-- Creating foreign key on [FORMULA_ID] in table 'TAXES'
ALTER TABLE [TAX].[TAXES]
ADD CONSTRAINT [FK_TAX_FORMULAS]
    FOREIGN KEY ([FORMULA_ID])
    REFERENCES [TAX].[FORMULAS]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TAX_FORMULAS'
CREATE INDEX [IX_FK_TAX_FORMULAS]
ON [TAX].[TAXES]
    ([FORMULA_ID]);
GO

-- Creating foreign key on [FORMULA_ID] in table 'TAXCONDS'
ALTER TABLE [TAX].[TAXCONDS]
ADD CONSTRAINT [FK_TAXCONDS_FORMULAS]
    FOREIGN KEY ([FORMULA_ID])
    REFERENCES [TAX].[FORMULAS]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TAXCONDS_FORMULAS'
CREATE INDEX [IX_FK_TAXCONDS_FORMULAS]
ON [TAX].[TAXCONDS]
    ([FORMULA_ID]);
GO

-- Creating foreign key on [VALUETYPE_ID] in table 'OPERATORS'
ALTER TABLE [TAX].[OPERATORS]
ADD CONSTRAINT [FK_OPERATORS_VALUETYPES]
    FOREIGN KEY ([VALUETYPE_ID])
    REFERENCES [TAX].[VALUETYPES]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_OPERATORS_VALUETYPES'
CREATE INDEX [IX_FK_OPERATORS_VALUETYPES]
ON [TAX].[OPERATORS]
    ([VALUETYPE_ID]);
GO

-- Creating foreign key on [ORDERTYPE_ID] in table 'TAXES'
ALTER TABLE [TAX].[TAXES]
ADD CONSTRAINT [FK_TAX_ORDERTYPE]
    FOREIGN KEY ([ORDERTYPE_ID])
    REFERENCES [TAX].[ORDERTYPES]
        ([ORDERTYPE_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TAX_ORDERTYPE'
CREATE INDEX [IX_FK_TAX_ORDERTYPE]
ON [TAX].[TAXES]
    ([ORDERTYPE_ID]);
GO

-- Creating foreign key on [TAX_ID] in table 'TAXCONDS'
ALTER TABLE [TAX].[TAXCONDS]
ADD CONSTRAINT [FK_TAXCOND_TAX]
    FOREIGN KEY ([TAX_ID])
    REFERENCES [TAX].[TAXES]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TAXCOND_TAX'
CREATE INDEX [IX_FK_TAXCOND_TAX]
ON [TAX].[TAXCONDS]
    ([TAX_ID]);
GO

-- Creating foreign key on [TAXCOND_ID] in table 'TAXEXCLUSIONS'
ALTER TABLE [TAX].[TAXEXCLUSIONS]
ADD CONSTRAINT [FK_TAXEXCLUSIONS_TAXCOND]
    FOREIGN KEY ([TAXCOND_ID])
    REFERENCES [TAX].[TAXCONDS]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TAXEXCLUSIONS_TAXCOND'
CREATE INDEX [IX_FK_TAXEXCLUSIONS_TAXCOND]
ON [TAX].[TAXEXCLUSIONS]
    ([TAXCOND_ID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------