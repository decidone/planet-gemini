using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    // �ǹ� ���� ��ũ��Ʈ
    // Updateó�� �Լ� ȣ���ϴ� �κ��� �� ���� Ŭ������ ���� ��
    // ����� ������ Ȯ�� ��� 1. �ݶ��̴�, 2. �ʿ��� ���� Ÿ�� üũ

    [SerializeField]
    protected int maxHp;
    [SerializeField]
    protected int hp;

    protected void ConveyorCheck()
    {
        // ����� �����̾� ��Ʈ üũ
    }

    protected void PipeCheck()
    {
        // ����� ������ üũ
    }

    protected void StructureCheck()
    {
        // ����� �ǹ� üũ
    }
}
