using UnityEngine;
using UnityEditor; // 에디터 툴을 만들기 위한 필수 네임스페이스
using System.IO;

public class CSVImporterWindow : EditorWindow
{
    private TextAsset csvFile;

    // 유니티 상단 메뉴에 [Tools -> 몬스터 데이터 자동 임포트] 버튼을 만듭니다.
    [MenuItem("Tools/몬스터 데이터 자동 임포트")]
    public static void ShowWindow()
    {
        // 팝업 창 띄우기
        GetWindow<CSVImporterWindow>("CSV to SO 임포터");
    }

    // 팝업 창 안의 UI를 그리는 함수
    private void OnGUI()
    {
        GUILayout.Label("CSV 파일을 SO로 일괄 변환합니다.", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // CSV 파일을 드래그 앤 드롭으로 받을 수 있는 칸
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV 파일", csvFile, typeof(TextAsset), false);
        GUILayout.Space(20);

        // 변환 실행 버튼
        if (GUILayout.Button("SO 파일 생성/업데이트", GUILayout.Height(40)))
        {
            if (csvFile != null)
            {
                ImportCSV();
            }
            else
            {
                EditorUtility.DisplayDialog("경고", "CSV 파일을 먼저 등록해주세요!", "확인");
            }
        }
    }

    private void ImportCSV()
    {
        // 1. 데이터를 저장할 폴더가 없으면 자동 생성
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Data"))
            AssetDatabase.CreateFolder("Assets/Resources", "Data");

        // 2. CSV 텍스트 줄바꿈 단위로 쪼개기
        string[] lines = csvFile.text.Split('\n');

        // 3. 첫 줄(컬럼명)은 건너뛰고 2번째 줄(i=1)부터 읽기
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // 쉼표(,) 기준으로 칸 나누기
            string[] values = line.Split(',');

            // 4. SO 파일 이름 및 경로 설정 (예: Assets/Resources/Data/MON_BAT.asset)
            string assetPath = $"Assets/Resources/Data/{values[0]}.asset";

            // 기존에 파일이 있는지 확인 후 덮어쓰거나 새로 생성
            NewMonsterData monsterData = AssetDatabase.LoadAssetAtPath<NewMonsterData>(assetPath);
            bool isNew = false;

            if (monsterData == null)
            {
                monsterData = ScriptableObject.CreateInstance<NewMonsterData>();
                isNew = true;
            }

            // 5. 엑셀 데이터 매핑 (인덱스 순서는 기획서 CSV 순서와 동일해야 함)
            monsterData.monsterId = values[0];
            monsterData.monsterName = values[1];
            monsterData.maxHp = float.Parse(values[2]);
            monsterData.moveSpeed = float.Parse(values[3]);
            monsterData.contactDamage = float.Parse(values[4]);
            monsterData.expReward = int.Parse(values[5]);

            // 7번째 칸(values[6])에 스케일 값이 있다면 파싱, 없다면 기본값 1.0 적용
            if (values.Length > 6 && !string.IsNullOrEmpty(values[6]))
            {
                monsterData.scaleMultiplier = float.Parse(values[6]);
            }
            else
            {
                monsterData.scaleMultiplier = 1.0f;
            }

            // 6. 에셋 저장
            if (isNew)
            {
                AssetDatabase.CreateAsset(monsterData, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(monsterData);
            }
        }

        // 7. 유니티 프로젝트 창 새로고침
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("성공", "CSV 임포트가 완료되었습니다!\n(Resources/Data 폴더 확인)", "확인");
    }
}