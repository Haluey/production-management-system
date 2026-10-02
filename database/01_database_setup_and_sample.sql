-- ============================================================
-- 생산 작업지시·실적 관리 프로그램
-- DB: SQL Server 2022 / ProductionManagementDb
-- 테이블 관계: Products → WorkOrders → ProductionResults
-- 등록·시작·완료 시각은 UTC로 저장하고, 화면에서 한국 시각으로 표시
-- 예정일·생산일은 업무상 날짜를 저장
-- ============================================================

-- 1. 프로젝트용 데이터베이스 생성
CREATE DATABASE ProductionManagementDb;

-- 데이터베이스 생성 여부 확인: state_desc가 ONLINE이면 정상
SELECT name, state_desc
FROM sys.databases
WHERE name = N'ProductionManagementDb';

-- 이후 SQL을 실행할 데이터베이스 선택
USE ProductionManagementDb;

-- 현재 선택된 데이터베이스 확인
SELECT DB_NAME() AS CurrentDatabase;

-- ============================================================
-- 2. 제품 테이블 생성
-- IDENTITY(1,1): 1부터 시작해 1씩 증가하는 자동 번호
-- PRIMARY KEY: 각 행을 구분하는 기본 키
-- UNIQUE: 중복 값 차단
-- DEFAULT: 값을 생략하면 기본값 적용
-- CHECK: 저장할 값의 조건 검사
-- NVARCHAR 및 N'문자열': 한글 등 유니코드 데이터 사용
-- ============================================================

CREATE TABLE dbo.Products (
    ProductId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY, -- 제품 내부 번호
    ProductCode NVARCHAR(30) NOT NULL CONSTRAINT UQ_Products_ProductCode UNIQUE, -- 중복 불가 제품 코드
    ProductName NVARCHAR(100) NOT NULL, -- 제품명
    Unit NVARCHAR(20) NOT NULL CONSTRAINT DF_Products_Unit DEFAULT N'EA', -- 수량 단위
    IsActive BIT NOT NULL CONSTRAINT DF_Products_IsActive DEFAULT 1, -- 1: 사용, 0: 비활성
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT SYSUTCDATETIME(), -- 등록 시각
    CONSTRAINT CK_Products_ProductCode_NotBlank CHECK (LEN(LTRIM(RTRIM(ProductCode))) > 0), -- 빈 제품 코드 차단
    CONSTRAINT CK_Products_ProductName_NotBlank CHECK (LEN(LTRIM(RTRIM(ProductName))) > 0), -- 빈 제품명 차단
    CONSTRAINT CK_Products_Unit_NotBlank CHECK (LEN(LTRIM(RTRIM(Unit))) > 0) -- 빈 단위 차단
);

-- Products 테이블이 프로젝트 DB에 생성됐는지 확인
SELECT TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME
FROM ProductionManagementDb.INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = N'dbo' AND TABLE_NAME = N'Products';

-- ============================================================
-- 3. 작업지시 테이블 생성
-- 어떤 제품을 언제 몇 개 생산할지 저장
-- 상태: Waiting(대기) → InProgress(진행 중) → Completed(완료)
-- FOREIGN KEY: 연결된 제품이 Products에 존재하도록 보장
-- 상태 전환 순서와 실적 등록 가능 여부는 이후 C#에서 구현
-- ============================================================

CREATE TABLE dbo.WorkOrders (
    WorkOrderId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkOrders PRIMARY KEY, -- 작업지시 내부 번호
    WorkOrderNo NVARCHAR(30) NOT NULL CONSTRAINT UQ_WorkOrders_WorkOrderNo UNIQUE, -- 중복 불가 작업지시 번호
    ProductId INT NOT NULL, -- 생산할 제품의 내부 번호
    PlannedDate DATE NOT NULL, -- 생산 예정일
    TargetQuantity INT NOT NULL, -- 목표 생산 수량
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_WorkOrders_Status DEFAULT N'Waiting', -- 기본 상태: 대기
    StartedAt DATETIME2(0) NULL, -- 시작 전에는 NULL
    CompletedAt DATETIME2(0) NULL, -- 완료 전에는 NULL
    Memo NVARCHAR(500) NULL, -- 선택 입력 비고
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_WorkOrders_CreatedAt DEFAULT SYSUTCDATETIME(), -- 등록 시각
    CONSTRAINT FK_WorkOrders_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId), -- 제품 연결
    CONSTRAINT CK_WorkOrders_WorkOrderNo_NotBlank CHECK (LEN(LTRIM(RTRIM(WorkOrderNo))) > 0), -- 빈 번호 차단
    CONSTRAINT CK_WorkOrders_TargetQuantity CHECK (TargetQuantity > 0), -- 목표 수량은 1 이상
    CONSTRAINT CK_WorkOrders_Status CHECK (Status IN (N'Waiting', N'InProgress', N'Completed')) -- 허용 상태 제한
);

-- ============================================================
-- 4. 생산실적 테이블 생성
-- 작업지시 하나에 여러 생산실적을 등록할 수 있음
-- 양품·불량 수량은 각각 0 이상이며, 동시에 0일 수는 없음
-- 합계와 달성률은 실적을 조회할 때 계산
-- ============================================================

CREATE TABLE dbo.ProductionResults (
    ProductionResultId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductionResults PRIMARY KEY, -- 실적 내부 번호
    WorkOrderId INT NOT NULL, -- 연결된 작업지시 내부 번호
    ProductionDate DATE NOT NULL, -- 실제 생산일
    GoodQuantity INT NOT NULL, -- 양품 수량
    DefectQuantity INT NOT NULL, -- 불량 수량
    Memo NVARCHAR(500) NULL, -- 불량 사유 등 비고
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ProductionResults_CreatedAt DEFAULT SYSUTCDATETIME(), -- 등록 시각
    CONSTRAINT FK_ProductionResults_WorkOrders FOREIGN KEY (WorkOrderId) REFERENCES dbo.WorkOrders(WorkOrderId), -- 작업지시 연결
    CONSTRAINT CK_ProductionResults_GoodQuantity CHECK (GoodQuantity >= 0), -- 음수 양품 차단
    CONSTRAINT CK_ProductionResults_DefectQuantity CHECK (DefectQuantity >= 0), -- 음수 불량 차단
    CONSTRAINT CK_ProductionResults_Quantity_NotZero CHECK (GoodQuantity > 0 OR DefectQuantity > 0) -- 모두 0인 실적 차단
);

-- 세 테이블 생성 여부 확인
SELECT TABLE_NAME
FROM ProductionManagementDb.INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = N'dbo'
  AND TABLE_TYPE = N'BASE TABLE'
ORDER BY TABLE_NAME;

-- ============================================================
-- 5. 테스트용 제품 3개 등록
-- IsActive와 CreatedAt은 생략했으므로 기본값 적용
-- ============================================================

INSERT INTO dbo.Products (ProductCode, ProductName, Unit)
VALUES
    (N'PRD-001', N'센서 모듈', N'EA'),
    (N'PRD-002', N'제어 보드', N'EA'),
    (N'PRD-003', N'전원 모듈', N'EA');

-- 등록된 제품 확인: 제품 3개, IsActive는 모두 1
SELECT ProductId, ProductCode, ProductName, Unit, IsActive
FROM dbo.Products
ORDER BY ProductId;

-- ============================================================
-- 6. 테스트용 작업지시 등록
-- 센서 모듈을 2026-10-02에 100개 생산하는 작업
-- 제품 코드로 ProductId를 찾아 연결
-- Status는 생략했으므로 Waiting으로 등록
-- ============================================================

INSERT INTO dbo.WorkOrders (WorkOrderNo, ProductId, PlannedDate, TargetQuantity, Memo)
SELECT N'WO-20261002-001', ProductId, '2026-10-02', 100, N'화면 및 기능 확인용 작업지시'
FROM dbo.Products
WHERE ProductCode = N'PRD-001';

-- 작업지시에 제품명을 연결해서 조회
-- INNER JOIN: 연결 조건이 일치하는 행을 조회
SELECT w.WorkOrderNo, p.ProductName, w.PlannedDate, w.TargetQuantity, w.Status
FROM dbo.WorkOrders AS w
INNER JOIN dbo.Products AS p ON p.ProductId = w.ProductId
WHERE w.WorkOrderNo = N'WO-20261002-001';

-- ============================================================
-- 7. 작업 시작
-- 대기 상태인 작업만 진행 중으로 변경하고 시작 시각 기록
-- ============================================================

UPDATE dbo.WorkOrders
SET Status = N'InProgress',
    StartedAt = SYSUTCDATETIME()
WHERE WorkOrderNo = N'WO-20261002-001'
  AND Status = N'Waiting';

-- 상태와 시작 시각 확인
SELECT WorkOrderNo, Status, StartedAt
FROM dbo.WorkOrders
WHERE WorkOrderNo = N'WO-20261002-001';

-- ============================================================
-- 8. 첫 번째 생산실적 등록
-- 진행 중인 작업에 양품 40개·불량 2개 등록
-- 주의: INSERT를 반복 실행하면 실적이 추가 등록됨
-- ============================================================

INSERT INTO dbo.ProductionResults (WorkOrderId, ProductionDate, GoodQuantity, DefectQuantity, Memo)
SELECT WorkOrderId, '2026-10-02', 40, 2, N'첫 번째 생산실적'
FROM dbo.WorkOrders
WHERE WorkOrderNo = N'WO-20261002-001'
  AND Status = N'InProgress';

-- 작업지시별 개별 실적 조회
SELECT w.WorkOrderNo, r.ProductionDate, r.GoodQuantity, r.DefectQuantity, r.Memo
FROM dbo.ProductionResults AS r
INNER JOIN dbo.WorkOrders AS w ON w.WorkOrderId = r.WorkOrderId
WHERE w.WorkOrderNo = N'WO-20261002-001'
ORDER BY r.ProductionResultId;

-- ============================================================
-- 9. 목표 대비 생산 현황 집계
-- SUM: 여러 실적의 수량 합산
-- BIGINT 변환: INT 범위를 넘는 누적 합계에 대비
-- COALESCE: 실적이 없어서 NULL인 합계를 0으로 표시
-- LEFT JOIN: 실적이 없는 작업지시도 조회에 포함
-- 달성률 = 양품 합계 / 목표 수량 × 100
-- 첫 실적 등록 후 예상 결과: 목표 100 / 양품 40 / 불량 2 / 달성률 40%
-- ============================================================

SELECT w.WorkOrderNo,
       p.ProductName,
       w.TargetQuantity,
       COALESCE(SUM(CAST(r.GoodQuantity AS BIGINT)), 0) AS TotalGoodQuantity,
       COALESCE(SUM(CAST(r.DefectQuantity AS BIGINT)), 0) AS TotalDefectQuantity,
       CAST(COALESCE(SUM(CAST(r.GoodQuantity AS BIGINT)), 0) * 100.0 / w.TargetQuantity AS DECIMAL(10,2)) AS AchievementRate
FROM dbo.WorkOrders AS w
INNER JOIN dbo.Products AS p ON p.ProductId = w.ProductId
LEFT JOIN dbo.ProductionResults AS r ON r.WorkOrderId = w.WorkOrderId
WHERE w.WorkOrderNo = N'WO-20261002-001'
GROUP BY w.WorkOrderNo, p.ProductName, w.TargetQuantity;

-- ============================================================
-- 10. 두 번째 생산실적 등록
-- 같은 작업에 양품 60개·불량 1개 추가
-- 주의: 이 INSERT도 한 번만 실행
-- ============================================================

INSERT INTO dbo.ProductionResults (WorkOrderId, ProductionDate, GoodQuantity, DefectQuantity, Memo)
SELECT WorkOrderId, '2026-10-02', 60, 1, N'두 번째 생산실적'
FROM dbo.WorkOrders
WHERE WorkOrderNo = N'WO-20261002-001'
  AND Status = N'InProgress';

-- 두 실적의 합계 확인
-- 예상 결과: 목표 100 / 양품 100 / 불량 3 / 달성률 100%
SELECT w.WorkOrderNo,
       p.ProductName,
       w.TargetQuantity,
       COALESCE(SUM(CAST(r.GoodQuantity AS BIGINT)), 0) AS TotalGoodQuantity,
       COALESCE(SUM(CAST(r.DefectQuantity AS BIGINT)), 0) AS TotalDefectQuantity,
       CAST(COALESCE(SUM(CAST(r.GoodQuantity AS BIGINT)), 0) * 100.0 / w.TargetQuantity AS DECIMAL(10,2)) AS AchievementRate
FROM dbo.WorkOrders AS w
INNER JOIN dbo.Products AS p ON p.ProductId = w.ProductId
LEFT JOIN dbo.ProductionResults AS r ON r.WorkOrderId = w.WorkOrderId
WHERE w.WorkOrderNo = N'WO-20261002-001'
GROUP BY w.WorkOrderNo, p.ProductName, w.TargetQuantity;

-- ============================================================
-- 11. 작업 완료
-- 진행 중인 작업을 완료로 변경하고 완료 시각 기록
-- 목표 달성만으로 자동 완료되는 것이 아니라 명시적으로 완료 처리
-- ============================================================

UPDATE dbo.WorkOrders
SET Status = N'Completed',
    CompletedAt = SYSUTCDATETIME()
WHERE WorkOrderNo = N'WO-20261002-001'
  AND Status = N'InProgress';

-- 최종 상태 확인: Completed, 시작·완료 시각 모두 존재
SELECT WorkOrderNo, Status, StartedAt, CompletedAt
FROM dbo.WorkOrders
WHERE WorkOrderNo = N'WO-20261002-001';