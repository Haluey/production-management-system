using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ProductionManagement.Wpf.Models;
using ProductionManagement.Wpf.Services;

namespace ProductionManagement.Wpf.ViewModels;

// 제품 목록 화면에서 사용할 데이터와 조회 기능
public sealed class ProductListViewModel {
    private readonly ProductService _productService;

    // 화면에 표시할 제품 목록
    // 항목이 추가되거나 제거되면 화면에도 변경을 알림
    public ObservableCollection<Product> Products { get; } = new();

    public ProductListViewModel(ProductService productService) {
        _productService = productService
            ?? throw new ArgumentNullException(nameof(productService));
    }

    // DB에서 제품을 조회한 뒤 화면용 목록에 담기
    public async Task LoadAsync() {
        var products = await _productService.GetAllAsync();

        Products.Clear();

        foreach (var product in products) {
            Products.Add(product);
        }
    }
}