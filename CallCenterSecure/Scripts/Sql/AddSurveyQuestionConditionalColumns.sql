/* Add missing conditional columns to SurveyQuestion table
   Run this script against your application database (e.g., AllianceCRM_Old) using SSMS or sqlcmd.
   After running, restart the application.
*/

IF COL_LENGTH('dbo.SurveyQuestion', 'ConditionalParentQuestionIndex') IS NULL
BEGIN
	ALTER TABLE dbo.SurveyQuestion
	ADD ConditionalParentQuestionIndex INT NULL;
END

IF COL_LENGTH('dbo.SurveyQuestion', 'ConditionalParentOptionText') IS NULL
BEGIN
	ALTER TABLE dbo.SurveyQuestion
	ADD ConditionalParentOptionText NVARCHAR(500) NULL;
END
