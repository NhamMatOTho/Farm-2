using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NPCPath))]
public class NPCSchedule : MonoBehaviour
{
    [SerializeField] private SO_NPCScheduleEventList so_NPCScheduleEventList = null;
    private SortedSet<NPCScheduleEvent> npcScheduleEventSet;
    private NPCPath npcPath;

    private void Awake()
    {
        npcScheduleEventSet = new SortedSet<NPCScheduleEvent>(new NPCScheduleEventSort());

        foreach(NPCScheduleEvent npcScheduleEvent in so_NPCScheduleEventList.npcScheduleEventList)
        {
            npcScheduleEventSet.Add(npcScheduleEvent);
        }

        npcPath = GetComponent<NPCPath>();
    }

    private void OnEnable()
    {
        EventHandler.AdvanceGameMinuteEvent += GameTimeSystem_AdvanceMinute;
    }

    private void OnDisable()
    {
        EventHandler.AdvanceGameMinuteEvent -= GameTimeSystem_AdvanceMinute;
    }

    private void GameTimeSystem_AdvanceMinute(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute, int gameSecond)
    {
        int time = (gameHour * 100) + gameMinute;

        NPCScheduleEvent matchingNPCScheduleEvent = null;

        foreach(NPCScheduleEvent npcScheduleEvent in  npcScheduleEventSet)
        {
            if(npcScheduleEvent.Time ==  time)
            {
                if (npcScheduleEvent.day != 0 && npcScheduleEvent.day != gameDay)
                    continue;

                if (npcScheduleEvent.season != Season.none && npcScheduleEvent.season != gameSeason)
                    continue;

                if(npcScheduleEvent.weather != Weather.none &&  npcScheduleEvent.weather != GameManager.Instance.currentWeather)
                    continue;

                matchingNPCScheduleEvent = npcScheduleEvent;
                break;
            }
            else if(npcScheduleEvent.Time > time)
            {
                break;
            }
        }

        if(matchingNPCScheduleEvent != null)
        {
            npcPath.BuildPath(matchingNPCScheduleEvent);
        }
    }
}
