using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ProductionManagement.Wpf.Data;
using ProductionManagement.Wpf.Models;
using System.Linq;

namespace ProductionManagement.Wpf.Services;

public sealed class ProductService {
    private readonly ProductRepository _productRepository;

    public ProductService(ProductRepository productRepository) {
        _productRepository = productRepository
            ?? throw new ArgumentNullException(nameof(productRepository));
    }

    // 전체 제품 목록 조회
    public Task<List<Product>> GetAllAsync() {
        return _productRepository.GetAllAsync();
    }

    // 입력값을 검사한 뒤 새 제품 등록
    public async Task<int> CreateAsync(
        string productCode,
        string productName,
        string unit) {
        // 앞뒤 공백 제거
        productCode = (productCode ?? string.Empty).Trim();
        productName = (productName ?? string.Empty).Trim();
        unit = (unit ?? string.Empty).Trim();

        // 필수 입력 및 DB 열의 최대 길이 검사
        if (string.IsNullOrWhiteSpace(productCode))
            throw new ArgumentException("제품 코드를 입력해 주세요.");

        if (productCode.Length > 30)
            throw new ArgumentException("제품 코드는 30자 이하로 입력해 주세요.");

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("제품명을 입력해 주세요.");

        if (productName.Length > 100)
            throw new ArgumentException("제품명은 100자 이하로 입력해 주세요.");

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("단위를 입력해 주세요.");

        if (unit.Length > 20)
            throw new ArgumentException("단위는 20자 이하로 입력해 주세요.");

        try {
            return await _productRepository.CreateAsync(
                productCode,
                productName,
                unit);
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627) {
            // DB의 고유 제약 조건으로 제품 코드 중복 차단
            throw new InvalidOperationException(
                "이미 등록된 제품 코드입니다.",
                ex);
        }
    }

    // 제품명과 단위를 검사한 뒤 수정
    public async Task UpdateAsync(
        int productId,
        string productName,
        string unit) {
        if (productId <= 0)
            throw new ArgumentException("수정할 제품을 선택해 주세요.");

        // 앞뒤 공백 제거
        productName = (productName ?? string.Empty).Trim();
        unit = (unit ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("제품명을 입력해 주세요.");

        if (productName.Length > 100)
            throw new ArgumentException("제품명은 100자 이하로 입력해 주세요.");

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("단위를 입력해 주세요.");

        if (unit.Length > 20)
            throw new ArgumentException("단위는 20자 이하로 입력해 주세요.");

        bool updated = await _productRepository.UpdateAsync(
            productId,
            productName,
            unit);

        // 해당 ID의 제품이 없어 수정되지 않은 경우
        if (!updated) {
            throw new InvalidOperationException(
                "수정할 제품을 찾을 수 없습니다. 제품 목록을 다시 조회해 주세요.");
        }
    }

    // 제품 사용 여부 변경
    public async Task SetActiveAsync(
        int productId,
        bool isActive) {
        if (productId <= 0)
            throw new ArgumentException("제품을 선택해 주세요.");

        bool updated = await _productRepository.SetActiveAsync(
            productId,
            isActive);

        // 대상 제품이 없어 변경되지 않은 경우
        if (!updated) {
            throw new InvalidOperationException(
                "제품을 찾을 수 없습니다. 제품 목록을 다시 조회해 주세요.");
        }
    }

    // 새 작업지시에 사용할 활성 제품만 조회
    public async Task<List<Product>> GetActiveAsync() {
        var products = await _productRepository.GetAllAsync();

        return products
            .Where(product => product.IsActive)
            .ToList();
    }
}