using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

//interface này dùng để phân biệt task có logic phức tạp hơn cần phải tự setup sẵn ở client
public interface ITaskLogic
{
    int Id { get; }
    string Name { get;} 
    void OnGo(TaskCompletionSource<bool> tcs);
    void OnClaim(TaskCompletionSource<bool> tcs);
}

//interface này dùng để phân biệt task chỉ đơn giản bấm vô link nhận thưởng
// ko đc tạo sẵn ở client mà đc tạo từ file json server trả về
public interface ITaskNewLogic
{

}