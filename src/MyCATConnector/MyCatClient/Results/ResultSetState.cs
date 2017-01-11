namespace MyCat.Data.MyCatClient.Results
{
	internal enum ResultSetState
	{
		None,
		ReadResultSetHeader,
		ReadingRows,
		HasMoreData,
		NoMoreData,
	}
}