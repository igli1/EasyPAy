namespace Application.Interfaces;

public interface IExcelReader<T>
{
    IEnumerable<T> ReadRows(Stream stream);
}