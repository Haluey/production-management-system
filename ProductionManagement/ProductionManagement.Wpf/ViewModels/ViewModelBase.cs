using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProductionManagement.Wpf.ViewModels;

// 속성 값의 변경을 화면에 알리는 공통 클래스
public abstract class ViewModelBase : INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null) {
        // 기존 값과 같으면 변경하지 않음
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;

        // 해당 속성의 값이 변경됐다고 화면에 알림
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));

        return true;
    }
}