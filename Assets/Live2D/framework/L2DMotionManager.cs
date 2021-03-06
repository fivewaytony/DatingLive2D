/**
 *
 *  You can modify and use this source freely
 *  only for the development of application related Live2D.
 *
 *  (c) Live2D Inc. All rights reserved.
 */
using System.Collections;
using live2d;

namespace live2d.framework
{
    /*
     * L2DMotionManager은 우선 순위(priority)를 지정하여 모션의 재생을 관리하기위한 클래스입니다.
     *
     * 주로 부모 클래스의 MotionQueueManager(표준 Live2D 라이브러리)에 부족한 다음의 기능을 보완합니다.
     *
     * 1. 인터럽트 제어
     *
     * 부모 클래스 MotionQueueManager는 startMotion을 호출하면 새로운 모션이 인터럽트로 시작하여
     * 기존의 모션이 종료됩니다. (전후의 모션은 매끄럽게 연결됩니다)
     *
     * L2DMotionManager에서는 대사 등의 인터럽트 당하고 싶지 않은 모션의 경우 인터럽트를 막아주는 구조를 제공합니다.
     * priority가 같은 경우는 인터럽트가 발생하지 않고 새로운 모션을 무시합니다.
     *
     * ２．음성 로드와의 연계
     *
     * 탭 등의 이벤트가 발생했을 때 음성 로드가 완료되지 않은 채 모션을 즉시 시작하면
     * 어긋나 버리는 경우가 있습니다. 이러한 경우를 위해 다음 프레임 이후부터 재생을 예약하는
     * 구조를 제공합니다.
     */
    public class L2DMotionManager : MotionQueueManager
    {
        // 메인 모션의 우선순위
        // 표준설정 0:재생되지 않음 1:아이들링(인터럽트 가능) 2:일반(보통은 인터럽트 없음) 3:강제 시작
        private int currentPriority; // 현재 재생중인 모션의 우선 순위
        private int reservePriority; // 재생 예정인 모션의 우선순위. 재생 중에는 0이 된다. 모션파일을 별도 스레드에서 읽어들일 떄 기능。

        /*
         * 재생중인 모션의 우선 순위
         * @return
         */
        public int getCurrentPriority()
        {
            return currentPriority;
        }


        /*
         * 예약중인 모션의 우선순위
         * @return
         */
        public int getReservePriority()
        {
            return reservePriority;
        }


        /*
         * 다음에 재생하려는 모션의 우선순위를 넘겨 재생 예약 가능한 상황인지 판단한다.
         *
         * @param priority
         * @return
         */
        public bool reserveMotion(int priority)
        {
            if (reservePriority >= priority)
            {
                return false;// 재생 예약이 있다(다른 스레드에서 준비하고 있다)
            }
            if (currentPriority >= priority)
            {
                return false;// 재생중인 모션이 있다.
            }
            reservePriority = priority; // 모션 재생이 비동기인 경우 우선 순위를 먼저 설정하고 예약해둔다.
            return true;
        }


        /*
         * 모션을 예약한다.
         * @param val
         */
        public void setReservePriority(int val)
        {
            reservePriority = val;
        }


        public override bool updateParam(ALive2DModel model)
        {
            bool updated = base.updateParam(model);
            if (isFinished())
            {
                currentPriority = 0; // 재생 중 모션의 우선 순위 해제
            }
            return updated;
        }


        public int startMotionPrio(AMotion motion, int priority)
        {
            if (priority == reservePriority)
            {
                reservePriority = 0;// 예약 취소
            }
            currentPriority = priority; // 재생 중 모션의 우선순위를 결정
            return base.startMotion(motion, false); // 두 번째 인수는 모션 데이터를 자동으로 삭제할지 여부
        }
    }
}