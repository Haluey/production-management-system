using System;

namespace ProductionManagement.Wpf.Models;

// Products 테이블의 제품 한 건을 담는 클래스
public sealed class Product {
    public int ProductId { get; set; }

    public string ProductCode { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string Unit { get; set; } = "EA";

    public bool IsActive { get; set; } = true;

    // DB에 저장된 생성 시각(UTC)
    public DateTime CreatedAt { get; set; }
}