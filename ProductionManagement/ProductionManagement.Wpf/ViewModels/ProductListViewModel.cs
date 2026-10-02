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
    private Product? _selectedProduct;
    private string _editProductName = string.Empty;
    private string _editUnit = string.Empty;

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

    // 표에서 선택한 제품
    public Product? SelectedProduct {
        get => _selectedProduct;
        set {
            if (SetProperty(ref _selectedProduct, value)) {
                // 선택한 제품의 정보를 수정 입력칸에 표시
                EditProductName = value?.ProductName ?? string.Empty;
                EditUnit = value?.Unit ?? string.Empty;
            }
        }
    }

    // 수정할 제품명
    public string EditProductName {
        get => _editProductName;
        set => SetProperty(ref _editProductName, value);
    }

    // 수정할 단위
    public string EditUnit {
        get => _editUnit;
        set => SetProperty(ref _editUnit, value);
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

    // 선택한 제품의 제품명과 단위 수정
    public async Task UpdateAsync() {
        var selectedProduct = SelectedProduct;

        if (selectedProduct is null)
            throw new ArgumentException("수정할 제품을 선택해 주세요.");

        await _productService.UpdateAsync(
            selectedProduct.ProductId,
            EditProductName,
            EditUnit);
    }

    // 선택한 제품의 활성화·비활성화
    public async Task SetActiveAsync(bool isActive) {
        var selectedProduct = SelectedProduct;

        if (selectedProduct is null)
            throw new ArgumentException("제품을 선택해 주세요.");

        await _productService.SetActiveAsync(
            selectedProduct.ProductId,
            isActive);
    }
}