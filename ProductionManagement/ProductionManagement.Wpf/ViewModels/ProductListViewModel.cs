using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ProductionManagement.Wpf.Models;
using ProductionManagement.Wpf.Services;

namespace ProductionManagement.Wpf.ViewModels;

public sealed class ProductListViewModel : ViewModelBase {
    private readonly ProductService _productService;

    private string _productCode = string.Empty;
    private string _productName = string.Empty;
    private string _unit = "EA";

    // 화면에 표시할 제품 목록
    public ObservableCollection<Product> Products { get; } = new();

    // 제품 코드 입력값
    public string ProductCode {
        get => _productCode;
        set => SetProperty(ref _productCode, value);
    }

    // 제품명 입력값
    public string ProductName {
        get => _productName;
        set => SetProperty(ref _productName, value);
    }

    // 단위 입력값
    public string Unit {
        get => _unit;
        set => SetProperty(ref _unit, value);
    }

    public ProductListViewModel(ProductService productService) {
        _productService = productService
            ?? throw new ArgumentNullException(nameof(productService));
    }

    // 제품 목록 조회
    public async Task LoadAsync() {
        var products = await _productService.GetAllAsync();

        Products.Clear();

        foreach (var product in products) {
            Products.Add(product);
        }
    }

    // 입력한 제품을 등록
    public async Task<int> CreateAsync() {
        int productId = await _productService.CreateAsync(
            ProductCode,
            ProductName,
            Unit);

        // 저장에 성공한 경우에만 입력값 초기화
        ProductCode = string.Empty;
        ProductName = string.Empty;
        Unit = "EA";

        return productId;
    }
}