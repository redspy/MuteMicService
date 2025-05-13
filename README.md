# MuteMicService

마이크를 자동으로 감지하고 음소거하는 Windows 애플리케이션입니다. 이 서비스는 마이크가 연결될 때마다 자동으로 음소거하고, 주기적으로 상태를 모니터링하여 항상 음소거 상태를 유지합니다.

## 주요 기능

- 시스템에 연결된 모든 마이크를 자동으로 감지
- 마이크 연결 즉시 자동 음소거 적용
- 음소거가 실패할 경우 자동 재시도
- 간단한 UI를 통해 서비스 활성화/비활성화
- 마이크 상태 실시간 표시

## 스크린샷

(여기에 스크린샷 이미지 추가)

## 시스템 요구사항

- Windows 10 이상
- .NET Framework 4.8
- 관리자 권한 (마이크 제어 접근 권한 필요)

## 설치 방법

1. 최신 릴리스에서 MuteMicService.zip 파일을 다운로드합니다.
2. 압축을 해제하고 원하는 위치에 저장합니다.
3. MuteMicService.exe 실행합니다.

## 기술 스택

- C# / WPF (.NET Framework 4.8)
- MVVM 디자인 패턴
- NAudio 라이브러리 (오디오 디바이스 제어)

## 프로젝트 구조

MuteMicService 프로젝트는 MVVM(Model-View-ViewModel) 패턴을 따라 구조화되어 있습니다:

```
MuteMicService/
├── Models/
│   └── MicrophoneManager.cs       # 마이크 디바이스 제어 모델
├── ViewModels/
│   ├── ViewModelBase.cs           # 기본 ViewModel 클래스
│   └── MainViewModel.cs           # 메인 화면 ViewModel
├── Views/
│   └── MainWindow.xaml            # 메인 UI 화면
├── Services/
│   └── MicMuteService.cs          # 마이크 음소거 서비스
├── Controls/
│   ├── StatusDisplay.xaml         # 상태 표시 컨트롤
│   └── StatusDisplay.xaml.cs      # 상태 표시 컨트롤 코드
└── Utils/
    └── RelayCommand.cs            # 명령 패턴 구현
```

## 컴포넌트 상세 설명

### 1. MicrophoneManager (모델)

마이크 하드웨어 제어를 담당하는 모델 클래스입니다.

**주요 기능:**

- 시스템의 활성 마이크 디바이스 감지
- 마이크 음소거/음소거 해제
- 마이크 상태 정보 제공

**예제 코드:**

```csharp
// MicrophoneManager 사용 예제
var manager = new MicrophoneManager();
if (manager.Initialize())
{
    // 마이크 디바이스 감지됨
    string micName = manager.GetCurrentMicrophoneName();
    Console.WriteLine($"감지된 마이크: {micName}");

    // 마이크 음소거
    bool success = manager.MuteMicrophone();
    if (success)
    {
        Console.WriteLine("마이크가 음소거되었습니다.");
    }
}
else
{
    Console.WriteLine("마이크가 감지되지 않았습니다.");
}

// 리소스 정리
manager.Dispose();
```

### 2. MicMuteService (서비스)

마이크 상태를 지속적으로 모니터링하고 자동 음소거 기능을 제공하는 서비스 클래스입니다.

**주요 기능:**

- 백그라운드 모니터링 스레드
- 마이크 연결 감지 및 자동 음소거
- 음소거 실패 시 재시도 메커니즘
- 상태 변경 알림 이벤트

**예제 코드:**

```csharp
// MicMuteService 사용 예제
var manager = new MicrophoneManager();
var service = new MicMuteService(manager);

// 서비스 이벤트 구독
service.StatusChanged += (sender, status) => Console.WriteLine($"상태 변경: {status}");
service.MicrophoneDetectionChanged += (sender, detected) =>
    Console.WriteLine($"마이크 감지 상태: {(detected ? "연결됨" : "연결되지 않음")}");
service.MicrophoneMuteStateChanged += (sender, muted) =>
    Console.WriteLine($"마이크 음소거 상태: {(muted ? "음소거됨" : "음소거되지 않음")}");

// 서비스 설정 구성 (확인 간격, 초기 지연, 재시도 횟수)
service.ConfigureSettings(checkIntervalMs: 500, initialDelayMs: 200, retryAttempts: 3);

// 서비스 시작
service.Start();

// 어플리케이션 종료 시
service.Stop();
```

### 3. StatusDisplay (컨트롤)

애플리케이션 상태를 시각적으로 표시하는 재사용 가능한 WPF UserControl입니다.

**주요 기능:**

- 상태 텍스트 및 세부 정보 표시
- 색상으로 상태 표시 (성공, 경고, 오류 등)
- 커스터마이징 가능한 디자인

**XAML에서 사용 예제:**

```xml
<Window x:Class="YourNamespace.YourWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:controls="clr-namespace:MuteMicService.Controls">
    <Grid>
        <controls:StatusDisplay
            StatusText="서비스 활성화됨"
            DetailText="마이크가 음소거되었습니다."
            StatusColor="Green" />
    </Grid>
</Window>
```

**코드 비하인드 사용 예제:**

```csharp
// StatusDisplay 사용 예제
StatusDisplay display = new StatusDisplay();

// 기본 속성 설정
display.StatusText = "초기화 중...";
display.DetailText = "잠시 기다려주세요.";

// 헬퍼 메소드 사용
display.SetStatus("작업 완료", "모든 작업이 성공적으로 완료되었습니다.", StatusState.Success);

// 상태 변경 시
try
{
    // 일부 작업 수행
    display.SetStatus("진행 중", "작업을 처리하고 있습니다.", StatusState.Active);

    // 성공 시
    display.SetStatus("성공", "작업이 완료되었습니다.", StatusState.Success);
}
catch (Exception ex)
{
    // 오류 시
    display.SetStatus("오류 발생", ex.Message, StatusState.Error);
}
```

### 4. MainViewModel (뷰모델)

UI와 데이터 모델 및 서비스 사이의 상호작용을 관리하는 ViewModel입니다.

**주요 기능:**

- 컨트롤과 서비스 간 상태 동기화
- 사용자 명령 처리
- UI 상태 관리 및 갱신

**예제 코드:**

```csharp
// MainViewModel 사용 예제 (일반적으로 직접 인스턴스화하지 않음, UI에 바인딩됨)
public class YourViewModel : ViewModelBase
{
    private readonly MicrophoneManager _microphoneManager;
    private readonly MicMuteService _micMuteService;

    // 속성 및 명령을 View에 바인딩하여 사용
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }

    // 초기화 및 구독
    public YourViewModel()
    {
        _microphoneManager = new MicrophoneManager();
        _micMuteService = new MicMuteService(_microphoneManager);

        StartCommand = new RelayCommand(() => _micMuteService.Start());
        StopCommand = new RelayCommand(() => _micMuteService.Stop());

        // 이벤트 구독
        _micMuteService.StatusChanged += HandleStatusChanged;
    }

    private void HandleStatusChanged(object sender, string status)
    {
        // 상태 업데이트 로직
    }
}
```

### 5. RelayCommand (유틸리티)

MVVM 패턴에서 UI 이벤트를 처리하기 위한 명령 패턴의 구현체입니다.

**주요 기능:**

- 뷰와 뷰모델 간의 명령 바인딩 제공
- 명령 실행 가능 여부 확인
- 파라미터화된 명령 지원

**예제 코드:**

```csharp
// RelayCommand 사용 예제
public class SampleViewModel : ViewModelBase
{
    private bool _canExecute = true;

    // 간단한 명령
    public ICommand SimpleCommand { get; }

    // 조건부 명령
    public ICommand ConditionalCommand { get; }

    // 파라미터를 사용하는 명령
    public ICommand ParameterizedCommand { get; }

    public SampleViewModel()
    {
        SimpleCommand = new RelayCommand(ExecuteSimpleAction);

        ConditionalCommand = new RelayCommand(
            ExecuteConditionalAction,
            () => _canExecute  // 실행 가능 여부 확인
        );

        ParameterizedCommand = new RelayCommand<string>(
            parameter => ExecuteParameterizedAction(parameter)
        );
    }

    private void ExecuteSimpleAction()
    {
        // 액션 실행
    }

    private void ExecuteConditionalAction()
    {
        // 조건부 액션 실행
    }

    private void ExecuteParameterizedAction(string parameter)
    {
        // 파라미터화된 액션 실행
    }
}
```

## 사용된 외부 라이브러리

### NAudio (2.2.1)

NAudio는 .NET에서 오디오 장치를 제어하기 위한 오픈 소스 오디오 라이브러리입니다.

**설치 방법:**

```
PM> Install-Package NAudio -Version 2.2.1
```

**주요 기능:**

- 오디오 장치 열거 및 선택
- 오디오 입출력 제어
- 마이크 및 스피커 볼륨 제어
- 오디오 형식 변환

**패키지 구성:**

- **NAudio.Core**: 핵심 오디오 기능
- **NAudio.Wasapi**: Windows 오디오 세션 API 지원
- **NAudio.Asio**: ASIO 장치 지원

**MuteMicService에서의 사용:**

- 마이크 디바이스 열거 및 감지
- 마이크 음소거 상태 제어
- 오디오 종점(Endpoint) 볼륨 제어

## 애플리케이션 실행 흐름

1. 애플리케이션 시작 시 `MicrophoneManager`가 초기화됩니다.
2. `MainViewModel`은 `MicrophoneManager`와 `MicMuteService`의 인스턴스를 생성합니다.
3. UI가 준비되면 `MainViewModel`은 초기 마이크 상태를 확인합니다.
4. 사용자가 "서비스 활성화" 버튼을 클릭하면 `MicMuteService`가 시작됩니다.
5. `MicMuteService`는 백그라운드 스레드에서 마이크 상태를 모니터링합니다.
6. 마이크가 감지되면 서비스는 자동으로 음소거를 적용합니다.
7. 음소거 상태 변경이 감지되면 UI가 업데이트됩니다.
8. 사용자가 "서비스 비활성화" 버튼을 클릭하면 모니터링이 중지됩니다.

## 확장 및 사용자 정의

MuteMicService는 확장 가능하도록 설계되었습니다. 가능한 확장 사항은 다음과 같습니다:

- **마이크 선택 기능**: 여러 마이크 중 특정 마이크만 음소거하도록 설정
- **예약 설정**: 특정 시간에 자동으로 서비스를 활성화/비활성화
- **핫키 지원**: 글로벌 키보드 단축키로 음소거 제어
- **시스템 트레이 통합**: 백그라운드에서 실행되는 시스템 트레이 애플리케이션으로 확장

## 문제 해결

일반적인 문제 및 해결 방법:

1. **마이크가 감지되지 않음**

   - 마이크가 올바르게 연결되어 있는지 확인
   - Windows 설정에서 마이크 권한 확인
   - 다른 프로그램에서 마이크를 독점적으로 사용하고 있는지 확인

2. **음소거가 적용되지 않음**

   - 애플리케이션을 관리자 권한으로 실행
   - Windows 오디오 설정에서 마이크 드라이버 업데이트
   - 마이크 드라이버 재설치

3. **애플리케이션이 충돌함**
   - 최신 버전의 .NET Framework 설치
   - 이벤트 로그에서 오류 메시지 확인
   - 애플리케이션 재시작

## 기여 방법

이 프로젝트에 기여하려면 다음 단계를 따르세요:

1. 이 저장소를 포크합니다.
2. 새 브랜치를 생성합니다 (`git checkout -b feature/amazing-feature`).
3. 변경 사항을 커밋합니다 (`git commit -m 'Add some amazing feature'`).
4. 브랜치를 푸시합니다 (`git push origin feature/amazing-feature`).
5. Pull Request를 생성합니다.

## 라이선스

이 프로젝트는 MIT 라이선스로 배포됩니다 - 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.
