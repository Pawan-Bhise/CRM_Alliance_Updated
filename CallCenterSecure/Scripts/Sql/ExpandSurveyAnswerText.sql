IF OBJECT_ID(N'dbo.SurveyFormAnswer', N'U') IS NULL
    THROW 50001, 'Required table dbo.SurveyFormAnswer does not exist.', 1;

IF COL_LENGTH(N'dbo.SurveyFormAnswer', N'AnswerText') IS NULL
    THROW 50002, 'Required column dbo.SurveyFormAnswer.AnswerText does not exist.', 1;

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.SurveyFormAnswer')
      AND name = N'AnswerText'
      AND max_length <> -1
)
BEGIN
    ALTER TABLE dbo.SurveyFormAnswer
        ALTER COLUMN AnswerText nvarchar(max) NULL;
END;
