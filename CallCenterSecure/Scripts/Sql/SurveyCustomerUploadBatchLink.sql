IF COL_LENGTH('dbo.UploadJobs', 'SurveyTemplateTypeId') IS NULL
BEGIN
    ALTER TABLE dbo.UploadJobs ADD SurveyTemplateTypeId INT NULL;
    CREATE INDEX IX_UploadJobs_SurveyTemplateTypeId
        ON dbo.UploadJobs(SurveyTemplateTypeId);
    ALTER TABLE dbo.UploadJobs ADD CONSTRAINT FK_UploadJobs_SurveyTemplateType
        FOREIGN KEY (SurveyTemplateTypeId) REFERENCES dbo.SurveyTemplateType(Id);
END;
GO

IF COL_LENGTH('dbo.SurveyCustomerData', 'UploadJobId') IS NULL
BEGIN
    ALTER TABLE dbo.SurveyCustomerData ADD UploadJobId INT NULL;
    CREATE INDEX IX_SurveyCustomerData_UploadJobId
        ON dbo.SurveyCustomerData(UploadJobId);
    ALTER TABLE dbo.SurveyCustomerData ADD CONSTRAINT FK_SurveyCustomerData_UploadJobs
        FOREIGN KEY (UploadJobId) REFERENCES dbo.UploadJobs(UploadJobId);
END;
GO