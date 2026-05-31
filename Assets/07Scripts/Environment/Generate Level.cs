using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateLevel : MonoBehaviour
{
    public GameObject[] section;
    public Transform player;
    public float sectionLength = 50f; // Length of each section
    public int sectionsToKeepAhead = 3; // Always have 3 sections ahead

    private List<GameObject> activeSections = new List<GameObject>();
    private float nextSpawnZ = 50f;
    private float destroyOffset = 100f; // Destroy 2 sections behind

    void Start()
    {
        // Spawn initial sections
        for (int i = 0; i < sectionsToKeepAhead; i++)
        {
            SpawnSection();
        }
    }

    void Update()
    {
        // Spawn new section when player is close to the last section
        if (player.position.z > nextSpawnZ - (sectionsToKeepAhead * sectionLength))
        {
            SpawnSection();
        }

        // Destroy sections behind player
        DestroyOldSections();
    }

    void SpawnSection()
    {
        int secNum = Random.Range(0, section.Length);

        if (section[secNum] != null)
        {
            GameObject newSection = Instantiate(
                section[secNum],
                new Vector3(2, 0, nextSpawnZ),
                Quaternion.identity
            );

            activeSections.Add(newSection);
            nextSpawnZ += sectionLength;
        }
    }

    void DestroyOldSections()
    {
        for (int i = activeSections.Count - 1; i >= 0; i--)
        {
            if (activeSections[i] != null)
            {
                // Destroy section if it's more than destroyOffset behind player
                if (player.position.z - activeSections[i].transform.position.z > destroyOffset)
                {
                    Destroy(activeSections[i]);
                    activeSections.RemoveAt(i);
                }
            }
            else
            {
                activeSections.RemoveAt(i);
            }
        }
    }
}