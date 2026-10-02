using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductionManagement.Wpf.Data;
using ProductionManagement.Wpf.Models;

namespace ProductionManagement.Wpf.Services;

// 제품 관련 기능과 업무 규칙을 처리하는 클래스
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
}