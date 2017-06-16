# SqlTools

Read from the database just with slightly more ease.

## Get field values without dealing with DBNull

Instead of

{code:c#}
var command = new MySqlCommand("SELECT * FROM `Table`", connection);
var reader = command.ExecuteReader();

var boolValue = reader.IsDbNull(0) ? reader.GetBoolean(0) : false;
var dateTimeValue = reader.IsDbNull(1) ? reader.GetDateTime(1) : DateTime.MinValue;
var stringValue = reader.IsDbNull(2) ? reader.GetString(2) : string.Empty;
{code:c#}

Simply do

{code:c#}
var command = new MySqlCommand("SELECT * FROM `Table`", connection);
var reader = command.ExecuteReader();

var boolValue = SqlTools.GetBoolean(reader, 0);
var dateTimeValue = SqlTools.GetDateTime(reader, 1);
var stringValue = SqlTools.GetString(reader, 2);
{code:c#}

**Types and default values**

||Type||Use method||Value if DBNull||
|bool|GetBoolean|false|
|DateTime|GetDateTime|DateTime.MinValue|
|decimal|GetDecimal|0|
|int|GetInt32|0|
|string|GetString|string.Empty|

## Create parameters with less code

Instead of

{code:c#}
var command = new MySqlCommand("INSERR INTO `Table` SELECT ?parameter", connection);
var parameter = command.CreateParameter()
{
	ParameterName = "?parameter",
	Value = "asdf",
	DbType = DbType.String
};
{code:c#}

Simply do

{code:c#}
var command = new MySqlCommand("INSERR INTO `Table` SELECT ?parameter", connection);
command.CreateParameter("?parameter", "asdf");
{code:c#}

Types supported:

* bool
* DateTime
* int
* string
* object
	* Calls the object's ToString() method and passes in the value into the parameter. If object is null, the resulting field will be DBNull