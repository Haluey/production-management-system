using System;
using System.Windows;
using ProductionManagement.Wpf.Data;
using ProductionManagement.Wpf.Services;
using ProductionManagement.Wpf.ViewModels;

namespace ProductionManagement.Wpf;

public partial class MainWindow : Window {
    private readonly ProductListViewModel _viewModel;
    private bool _isBusy;

    public MainWindow() {
        InitializeComponent();

        var connectionFactory = new SqlConnectionFactory();
        var productRepository = new ProductRepository(connectionFactory);
        var productService = new ProductService(productRepository);

        _viewModel = new ProductListViewModel(productService);
        DataContext = _viewModel;
    }

    // 처리 중에는 입력, 제품 선택, 중복 실행을 막음
    private void SetBusy(bool isBusy) {
        _isBusy = isBusy;

        RefreshButton.IsEnabled = !isBusy;
        ProductInputPanel.IsEnabled = !isBusy;
        ProductEditPanel.IsEnabled = !isBusy;
        ProductGrid.IsEnabled = !isBusy;
    }

    private async void RefreshProducts_Click(
        object sender,
        RoutedEventArgs e) {
        if (_isBusy)
            return;

        SetBusy(true);

        try {
            await _viewModel.LoadAsync();
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(
                $"제품 조회 실패\n{ex.Message}",
                "조회 오류",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally {
            SetBusy(false);
        }
    }

    private async void CreateProduct_Click(
        object sender,
        RoutedEventArgs e) {
        if (_isBusy)
            return;

        SetBusy(true);

        try {
            // 입력 검사 및 DB 저장
            await _viewModel.CreateAsync();

            // 저장 성공 후 목록 새로고침
            try {
                await _viewModel.LoadAsync();
            }
            catch (Exception ex) {
                // 저장은 성공했으므로 다시 등록하지 않도록 안내
                System.Windows.MessageBox.Show(
                    $"제품은 등록됐지만 목록 갱신에 실패했습니다.\n"
                    + $"제품 조회 버튼을 눌러 주세요.\n{ex.Message}",
                    "목록 갱신 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            System.Windows.MessageBox.Show(
                "제품이 등록됐습니다.",
                "등록 완료",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (ArgumentException ex) {
            System.Windows.MessageBox.Show(
                ex.Message,
                "입력 확인",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (InvalidOperationException ex) {
            System.Windows.MessageBox.Show(
                ex.Message,
                "등록 확인",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(
                $"등록 처리 중 오류가 발생했습니다.\n"
                + $"제품 조회로 저장 여부를 확인해 주세요.\n{ex.Message}",
                "등록 오류",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally {
            SetBusy(false);
        }
    }

    private async void UpdateProduct_Click(
    object sender,
    RoutedEventArgs e) {
        if (_isBusy)
            return;

        SetBusy(true);

        try {
            await _viewModel.UpdateAsync();

            // 수정 성공 후 최신 목록 조회
            try {
                await _viewModel.LoadAsync();
            }
            catch (Exception ex) {
                System.Windows.MessageBox.Show(
                    $"제품은 수정됐지만 목록 갱신에 실패했습니다.\n"
                    + $"제품 조회 버튼을 눌러 주세요.\n{ex.Message}",
                    "목록 갱신 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            System.Windows.MessageBox.Show(
                "제품이 수정됐습니다.",
                "수정 완료",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (ArgumentException ex) {
            System.Windows.MessageBox.Show(
                ex.Message,
                "입력 확인",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (InvalidOperationException ex) {
            System.Windows.MessageBox.Show(
                ex.Message,
                "수정 확인",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex) {
            System.Windows.MessageBox.Show(
                $"수정 처리 중 오류가 발생했습니다.\n"
                + $"제품 조회로 저장 여부를 확인해 주세요.\n{ex.Message}",
                "수정 오류",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally {
            SetBusy(false);
        }
    }
}