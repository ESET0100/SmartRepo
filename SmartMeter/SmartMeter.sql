-- Database: SmartMeter

-- DROP DATABASE IF EXISTS "SmartMeter";

CREATE DATABASE "SmartMeter"
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'English_India.1252'
    LC_CTYPE = 'English_India.1252'
    LOCALE_PROVIDER = 'libc'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;


-- User table
CREATE TABLE "User" (  
  UserId         BIGSERIAL PRIMARY KEY,  
  Username       VARCHAR(100) NOT NULL UNIQUE,  
  PasswordHash   BYTEA NOT NULL,  
  DisplayName    VARCHAR(150) NOT NULL,  
  Email          VARCHAR(200) NULL,  
  Phone          VARCHAR(30) NULL,  
  LastLoginUtc   TIMESTAMP(3) NULL,  
  IsActive       BOOLEAN NOT NULL DEFAULT true
);

-- Organization Unit table
CREATE TABLE OrgUnit (
  OrgUnitId SERIAL PRIMARY KEY,
  Type VARCHAR(20) NOT NULL CHECK (Type IN ('Zone','Substation','Feeder','DTR')),
  Name VARCHAR(100) NOT NULL,
  ParentId INT NULL REFERENCES OrgUnit(OrgUnitId)
);

-- Tariff table
CREATE TABLE Tariff (
  TariffId SERIAL PRIMARY KEY,
  Name VARCHAR(100) NOT NULL,
  EffectiveFrom DATE NOT NULL,
  EffectiveTo DATE NULL,
  BaseRate DECIMAL(18,4) NOT NULL,
  TaxRate DECIMAL(18,4) NOT NULL DEFAULT 0,
  
  -- Constraints
  CONSTRAINT CHK_Tariff_EffectiveDates CHECK (EffectiveTo IS NULL OR EffectiveTo > EffectiveFrom),
  CONSTRAINT CHK_Tariff_BaseRate_Positive CHECK (BaseRate > 0)
);

-- Time of Day Rule table
CREATE TABLE TodRule (
  TodRuleId SERIAL PRIMARY KEY,
  TariffId INT NOT NULL REFERENCES Tariff(TariffId),
  Name VARCHAR(50) NOT NULL,
  StartTime TIME(0) NOT NULL,
  EndTime TIME(0) NOT NULL,
  RatePerKwh DECIMAL(18,6) NOT NULL,
  
  -- Constraints
  CONSTRAINT CHK_TodRule_TimeRange CHECK (EndTime > StartTime),
  CONSTRAINT CHK_TodRule_Rate_Positive CHECK (RatePerKwh > 0)
);

-- Tariff Slab table
CREATE TABLE TariffSlab (
  TariffSlabId SERIAL PRIMARY KEY,
  TariffId INT NOT NULL REFERENCES Tariff(TariffId),
  FromKwh DECIMAL(18,6) NOT NULL,
  ToKwh DECIMAL(18,6) NOT NULL,
  RatePerKwh DECIMAL(18,6) NOT NULL,
  
  -- Constraints
  CONSTRAINT CHK_TariffSlab_Range CHECK (FromKwh >= 0 AND ToKwh > FromKwh),
  CONSTRAINT CHK_TariffSlab_Rate_Positive CHECK (RatePerKwh > 0)
);

-- Consumer table
CREATE TABLE Consumer (
  ConsumerId BIGSERIAL PRIMARY KEY,
  Name VARCHAR(200) NOT NULL,
  Address VARCHAR(500) NULL,
  Phone VARCHAR(30) NULL,
  Email VARCHAR(200) NULL,
  OrgUnitId INT NOT NULL REFERENCES OrgUnit(OrgUnitId),
  TariffId INT NOT NULL REFERENCES Tariff(TariffId),
  Status VARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (Status IN ('Active','Inactive')),
  CreatedAt TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CreatedBy VARCHAR(100) NOT NULL DEFAULT 'system',
  UpdatedAt TIMESTAMP(3) NULL,
  UpdatedBy VARCHAR(100) NULL,
  
  -- Constraints
  CONSTRAINT CHK_Consumer_UpdatedAfterCreated CHECK (UpdatedAt IS NULL OR UpdatedAt >= CreatedAt)
);

-- Meter table
CREATE TABLE Meter (
  MeterSerialNo VARCHAR(50) NOT NULL PRIMARY KEY,
  IpAddress VARCHAR(45) NOT NULL,
  ICCID VARCHAR(30) NOT NULL,
  IMSI VARCHAR(30) NOT NULL,
  Manufacturer VARCHAR(100) NOT NULL,
  Firmware VARCHAR(50) NULL,
  Category VARCHAR(50) NOT NULL,
  InstallTsUtc TIMESTAMP(3) NOT NULL,
  Status VARCHAR(20) NOT NULL DEFAULT 'Active'
           CHECK (Status IN ('Active','Inactive','Decommissioned')),
  ConsumerId BIGINT NOT NULL REFERENCES Consumer(ConsumerId)
);

-- Meter Reading table for energy consumption data
CREATE TABLE MeterReading (
    ReadingId BIGSERIAL PRIMARY KEY,
    ReadingDate DATE NOT NULL,
    EnergyConsumed DECIMAL(18,6) NOT NULL,
    MeterSerialNo VARCHAR(50) NOT NULL REFERENCES Meter(MeterSerialNo),
    Current DECIMAL(8,3) NULL,
    Voltage DECIMAL(8,3) NULL,
    
    -- Constraints
    CONSTRAINT CHK_MeterReading_EnergyConsumed_Positive CHECK (EnergyConsumed >= 0),
    CONSTRAINT CHK_MeterReading_Voltage_Range CHECK (Voltage IS NULL OR (Voltage >= 0 AND Voltage <= 500)),
    CONSTRAINT CHK_MeterReading_Current_Range CHECK (Current IS NULL OR (Current >= 0 AND Current <= 500))
);

-- Tariff Details table (junction table for tariff calculations)
CREATE TABLE TariffDetails (
  TariffDetailId BIGSERIAL PRIMARY KEY,
  TariffId INT NOT NULL REFERENCES Tariff(TariffId),
  TariffSlabId INT NULL REFERENCES TariffSlab(TariffSlabId),
  TodRuleId INT NULL REFERENCES TodRule(TodRuleId)
);

--Billing Table
CREATE TABLE Billing (
    BillId SERIAL PRIMARY KEY,
    ConsumerId BIGINT NOT NULL REFERENCES Consumer(ConsumerId) ON DELETE CASCADE,
    MeterId VARCHAR(50) NOT NULL REFERENCES Meter(MeterSerialNo) ON DELETE CASCADE,
    BillingPeriodStart DATE NOT NULL,
    BillingPeriodEnd DATE NOT NULL,
    TotalUnitsConsumed NUMERIC(18,6) NOT NULL,
    BaseAmount NUMERIC(18,4) NOT NULL,
    TaxAmount NUMERIC(18,4) NOT NULL DEFAULT 0,
    TotalAmount NUMERIC(18,4) GENERATED ALWAYS AS (BaseAmount + TaxAmount) STORED,
    GeneratedAt TIMESTAMP(3) WITH TIME ZONE NOT NULL DEFAULT NOW(),
    DueDate DATE NOT NULL,
    PaidDate TIMESTAMP(3) WITH TIME ZONE,
    PaymentStatus VARCHAR(20) NOT NULL DEFAULT 'Unpaid'
                    CHECK (PaymentStatus IN ('Unpaid', 'Paid', 'Overdue', 'Cancelled')),
    DisconnectionDate TIMESTAMP(3) WITH TIME ZONE,
    
    -- Additional constraints
    CONSTRAINT CHK_Billing_Period CHECK (BillingPeriodEnd > BillingPeriodStart),
    CONSTRAINT CHK_DueDate_After_End CHECK (DueDate >= BillingPeriodEnd),
    CONSTRAINT CHK_BaseAmount_Positive CHECK (BaseAmount >= 0),
    CONSTRAINT CHK_TaxAmount_Positive CHECK (TaxAmount >= 0)
);

CREATE TABLE Arrears (
    ArrearId BIGSERIAL PRIMARY KEY,
    ConsumerId BIGINT NOT NULL REFERENCES Consumer(ConsumerId) ON DELETE CASCADE,
    ArrearType VARCHAR(50) NOT NULL CHECK (ArrearType IN ('interest', 'penalty', 'overdue', 'disconnected')),
    PaidStatus VARCHAR(20) NOT NULL CHECK (PaidStatus IN ('Pending', 'Paid', 'Partial')),
    BillId BIGINT NOT NULL REFERENCES Billing(BillId) ON DELETE CASCADE,
    ArrearAmount NUMERIC(18,2) NOT NULL,
    CreatedAt TIMESTAMP(3) WITH TIME ZONE DEFAULT NOW(),
    
    -- Constraints
    CONSTRAINT CHK_Arrears_Amount_Positive CHECK (ArrearAmount >= 0)
);

-- Drop the voltage range constraint
ALTER TABLE MeterReading DROP CONSTRAINT CHK_MeterReading_Voltage_Range;

-- Drop the current range constraint
ALTER TABLE MeterReading DROP CONSTRAINT CHK_MeterReading_Current_Range;

-- Add deleted column to TariffSlab table
ALTER TABLE TariffSlab ADD COLUMN deleted BOOLEAN NOT NULL DEFAULT false;

-- Add deleted column to Consumer table
ALTER TABLE Consumer ADD COLUMN deleted BOOLEAN NOT NULL DEFAULT false;

-- Add deleted column to TodRule table
ALTER TABLE TodRule ADD COLUMN deleted BOOLEAN NOT NULL DEFAULT false;

-- Create index on Type column
CREATE INDEX IDX_OrgUnit_Type ON OrgUnit(Type);

-- Create index on Name column  
CREATE INDEX IDX_OrgUnit_Name ON OrgUnit(Name);

-- First drop the existing constraint
ALTER TABLE TodRule DROP CONSTRAINT CHK_TodRule_Rate_Positive;
-- Add new constraint allowing zero or positive values
ALTER TABLE TodRule ADD CONSTRAINT CHK_TodRule_Rate_NonNegative CHECK (RatePerKwh >= 0);

-- First create the new Address table
CREATE TABLE Address (
    AddressId BIGSERIAL PRIMARY KEY,
    HouseNumber VARCHAR(50) NOT NULL,
    Locality VARCHAR(200) NOT NULL,
    City VARCHAR(100) NOT NULL,
    State VARCHAR(100) NOT NULL,
    Pincode VARCHAR(10) NOT NULL,
    ConsumerId BIGINT NOT NULL REFERENCES Consumer(ConsumerId),
    CreatedAt TIMESTAMP(3) DEFAULT CURRENT_TIMESTAMP,
    IsPrimary BOOLEAN NOT NULL DEFAULT true
);

-- Create index for better performance
CREATE INDEX IDX_Address_ConsumerId ON Address(ConsumerId);
CREATE INDEX IDX_Address_Pincode ON Address(Pincode);

ALTER TABLE Consumer DROP COLUMN Address;
ALTER TABLE Address DROP COLUMN IsPrimary;

-- Add the new constraint for ArrearType
ALTER TABLE Arrears ADD CONSTRAINT CHK_Arrears_ArrearType 
CHECK (ArrearType IN ('interest', 'penalty', 'overdue'));

-- Indexes for Billing table
CREATE INDEX IDX_Billing_ConsumerId ON Billing(ConsumerId);
CREATE INDEX IDX_Billing_MeterId ON Billing(MeterId);
CREATE INDEX IDX_Billing_PaymentStatus ON Billing(PaymentStatus);
CREATE INDEX IDX_Billing_Period ON Billing(BillingPeriodStart, BillingPeriodEnd);

-- Indexes for Arrears table
CREATE INDEX IDX_Arrears_ConsumerId ON Arrears(ConsumerId);
CREATE INDEX IDX_Arrears_BillId ON Arrears(BillId);
CREATE INDEX IDX_Arrears_PaidStatus ON Arrears(PaidStatus);


SELECT * FROM "User";
SELECT * FROM OrgUnit;
SELECT * FROM Tariff;
SELECT * FROM TodRule;
SELECT * FROM TariffSlab;
SELECT * FROM Consumer;
SELECT * FROM Address;
SELECT * FROM Meter;
SELECT * FROM MeterReading;
SELECT * FROM TariffDetails;
SELECT * FROM Billing;
SELECT * FROM Arrears;






