# GameScene 렌더링 최적화 보고서

작성일: 2026-10-30 · 대상: `Assets/01_Scenes/GameScene.unity` · Unity 6000.3.22f1 / URP 17

## 측정 방법
- 에디터 Play 모드에서 Game View를 1920×1080, Quality "PC"로 두고 측정했다.
- `Time.timeScale = 0`으로 시간을 멈추고, 좀비는 비활성화하고, `CameraViewSwitcher`를 끈 뒤 Main Camera를 아래 고정 시점에 놓았다.
- 렌더 수치는 `UnityStats` 값이다. Main 카메라와 Minimap 카메라, 그림자 패스를 모두 포함한다.
- 원본 데이터: `baseline.json` → `meshlod.json` → `lodgroup.json` → `occlusion.json`(최종). 스크린샷: `screens/<phase>_<view>.png`

| 시점 | 위치 → 바라보는 곳 | 의도 |
|---|---|---|
| view1_start | (16, 1.8, -26.6) → (16, 1.4, -10) | 플레이어 시작 위치의 1인칭 시야 |
| view2_street | (20, 1.7, 0) → (60, 3, 5) | 격납고 벽 바로 앞(시야가 가려짐) |
| view3_aerial | (-80, 40, -90) → (24, 0, -12) | 맵 전체를 내려다보는 원경 |

## 최종 비교 (Baseline → 최적화 후)

| 시점 | 지표 | Before | After | 변화 |
|---|---|---:|---:|---:|
| view1_start | Batches | 184 | 154 | **-16%** |
| | SetPass | 73 | 73 | 0% |
| | Draw Calls | 433 | 362 | -16% |
| | Triangles | 106,300 | 87,365 | -18% |
| | Vertices | 180,507 | 140,661 | -22% |
| view2_street | Batches | 195 | 81 | **-58%** |
| | SetPass | 68 | 58 | -15% |
| | Draw Calls | 474 | 127 | **-73%** |
| | Triangles | 97,251 | 19,646 | **-80%** |
| | Vertices | 185,333 | 41,038 | -78% |
| | Shadow Casters | 269 | 73 | -73% |
| view3_aerial | Batches | 241 | 228 | -5% |
| | SetPass | 71 | 71 | 0% |
| | Draw Calls | 736 | 656 | -11% |
| | Triangles | 4,147,953 | 150,776 | **-96%** |
| | Vertices | 2,603,908 | 269,523 | **-90%** |

## 기법별 기여도

| 단계 | view1 Tris / Draw | view2 Tris / Draw | view3 Tris / Draw |
|---|---|---|---|
| Baseline | 106,300 / 433 | 97,251 / 474 | 4,147,953 / 736 |
| + Mesh LOD | 106,498 / 433 | 97,362 / 474 | **154,789** / 738 |
| + LOD Group | 106,498 / 433 | 97,050 / 468 | 150,723 / **655** |
| + Occlusion Culling | **87,365 / 362** | **19,646 / 127** | 150,776 / 656 |

- **Mesh LOD**: 원경의 삼각형이 96% 줄었다. 씬 삼각형의 96%를 차지하던 50만 tri 메쉬 4개(트랭크스, 검, 하치와레, 스태프)에 LOD가 자동으로 15~16단계 생성되어(50만 → 26만 → 13.6만 … → 64), 멀리서는 낮은 LOD로 그려진다. SkinnedMeshRenderer에서도 정상 동작하는 것을 확인했다.
- **LOD Group**: 원경 Draw Call이 83개 줄었다. 맵 소품 301개 그룹에 Culled 단계를 넣어서, 작은 소품(<4m)은 화면 높이 2% 미만, 큰 구조물은 0.5% 미만이 되면 그리지 않는다. 별도의 저폴리 메쉬가 없는 에셋이라 삼각형 감소 효과는 작다.
- **Occlusion Culling**: 시야가 가려지는 시점에서 효과가 가장 컸다(view2 Draw Call -73%, Tris -80%). 큰 구조물(≥4m) 253개 렌더러는 Occluder+Occludee로, 작은 소품 149개는 Occludee 전용으로 설정하고 베이크했다(Smallest Occluder 2m, Hole 0.25m, 데이터 약 0.85MB). 원경(view3)은 가릴 것이 없어서 변화가 없다.

## 프레임 시간에 대해
Main Thread 중앙값은 모든 단계에서 10.0~11.0ms로 거의 같았다. 에디터 Play 모드는 에디터 자체의 오버헤드와 프레임 페이싱이 지배적이라 GPU 부하 감소가 이 값에 드러나지 않는다. 실제 프레임 개선을 확인하려면 **Standalone 빌드 + Profiler**로 GPU 시간을 측정하는 것이 정확하다. 이 보고서의 결론은 삼각형, 정점, Draw Call 기준이다.

## 발견 사항
- **하치와레 모델이 도로 바닥 아래에 묻혀 있다.** 위치는 (-6.5, 0, -8.2)이고, 그 지점의 `Road_set_v1_s_floor` 윗면은 y≈1.63이다. 게임 화면에는 보이지 않지만 최적화 전에는 50만 tri ×2와 그림자가 계속 렌더링되고 있었다. 지금은 Mesh LOD와 오클루전으로 비용이 크게 줄었다. 의도한 배치가 아니라면 위치를 옮기거나 비활성화하는 것이 좋다.
- 50만 tri 캐릭터는 가까이 있을 때 여전히 LOD0(50만)으로 그려진다. 근거리 비용까지 줄이려면 DCC(Blender 등)에서 원본을 데시메이트하는 것이 근본적인 해결책이다.
- 좀비 FBX(약 4.5천 tri)는 효과가 작고 `Zombie3.FBX.meta`에 기존 미커밋 변경이 있어서 Mesh LOD를 적용하지 않았다.

## 변경 파일
- `Assets/01_Scenes/GameScene.unity`: LODGroup 301개 추가, 맵 Static 플래그 조정
- `Assets/01_Scenes/GameScene/OcclusionCullingData.asset`: 새로 생성
- 50만 tri FBX 4개의 `.meta`: `generateMeshLods: 1`. 재임포트하면서 Unity가 meta 직렬화 버전도 함께 갱신했다.
- `Docs/Optimization/`: 측정 데이터, 스크린샷, 이 보고서
