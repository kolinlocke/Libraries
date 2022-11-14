Declare @TableName VarChar(50) = 'Customer'

Select
	'public ' + Tb.Ex_DataType + Tb.Ex_Nullable +  ' ' + Tb.COLUMN_NAME + ' { get; set;}' Prop	
From (
	Select		
		Tb.*
		, Case 
			When Is_Nullable = 'YES' And Tb.Ex_DataType != 'String' Then '?'
			Else ''
		End Ex_Nullable
	From (
		Select 
			Cols.* 		
			, Case 
				When Cols.Data_Type In ('bit') Then 'Boolean'
				When Cols.Data_Type In ('int') Then 'Int32'
				When Cols.Data_Type In ('bigint') Then 'Int64'
				When Cols.Data_Type In ('float') Then 'Double'
				When Cols.Data_Type In ('money') Then 'Decimal'
				When Cols.Data_Type In ('char','varchar','nvarchar') Then 'String'
				When Cols.Data_Type In ('DateTime') Then 'DateTime'
				When Cols.Data_Type In ('uniqueidentifier') Then 'Guid'
				Else 'Unmapped DataType: ' + Cols.Data_Type
			End Ex_DataType
		From 
			INFORMATION_SCHEMA.COLUMNS Cols
		Where
			Table_Name = @TableName
		) Tb) Tb
