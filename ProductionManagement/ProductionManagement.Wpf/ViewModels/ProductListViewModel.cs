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

    private bool _isEditing;
    private bool _isCreating = true;

    public ObservableCollection<Product> Products { get; } = new();

    // 등록·수정에서 함께 사용하는 입력값
    public string ProductCode {
        get => _productCode;
        set => SetProperty(ref _productCode, value);
    }

    public string ProductName {
        get => _productName;
        set => SetProperty(ref _productName, value);
    }

    public string Unit {
        get => _unit;
        set => SetProperty(ref _unit, value);
    }

    // 기존 제품 선택 중: 수정 가능, 제품 코드 변경 불가
    public bool IsEditing {
        get => _isEditing;
        private set => SetProperty(ref _isEditing, value);
    }

    // 새 제품 입력 중: 등록 가능
    public bool IsCreating {
        get => _isCreating;
        private set => SetProperty(ref _isCreating, value);
    }

    public Product? SelectedProduct {
        get => _selectedProduct;
        set {
            if (SetProperty(ref _selectedProduct, value)) {
                // 선택한 제품을 공통 입력칸에 표시
                ProductCode = value?.ProductCode ?? string.Empty;
                ProductName = value?.ProductName ?? string.Empty;
                Unit = value?.Unit ?? "EA";

                IsEditing = value is not null;
                IsCreating = value is null;
            }
        }
    }

    public ProductListViewModel(ProductService productService) {
        _productService = productService
            ?? throw new ArgumentNullException(nameof(productService));
    }

    // 조회 성공 후 선택과 입력을 초기화하고 목록 갱신
    public async Task LoadAsync() {
        var products = await _productService.GetAllAsync();

        ResetInput();
        Products.Clear();

        foreach (var product in products) {
            Products.Add(product);
        }
    }

    // 선택을 해제하고 새 제품 입력 상태로 전환
    public void ResetInput() {
        SelectedProduct = null;

        // 이미 선택이 없는 경우에도 입력값을 초기화
        ProductCode = string.Empty;
        ProductName = string.Empty;
        Unit = "EA";

        IsEditing = false;
        IsCreating = true;
    }

    // 공통 입력칸의 값으로 새 제품 등록
    public async Task<int> CreateAsync() {
        if (SelectedProduct is not null) {
            throw new ArgumentException(
                "입력 초기화를 누른 뒤 새 제품을 등록해 주세요.");
        }

        int productId = await _productService.CreateAsync(
            ProductCode,
            ProductName,
            Unit);

        // 등록에 성공한 경우에만 초기화
        ResetInput();

        return productId;
    }

    // 공통 입력칸의 값으로 선택한 제품 수정
    public async Task UpdateAsync() {
        var product = SelectedProduct;

        if (product is null)
            throw new ArgumentException("수정할 제품을 선택해 주세요.");

        await _productService.UpdateAsync(
            product.ProductId,
            ProductName,
            Unit);

        // 수정에 성공한 경우에만 초기화
        ResetInput();
    }

    // 선택한 제품의 활성화·비활성화
    public async Task SetActiveAsync(bool isActive) {
        var product = SelectedProduct;

        if (product is null)
            throw new ArgumentException("제품을 선택해 주세요.");

        await _productService.SetActiveAsync(
            product.ProductId,
            isActive);
    }
}