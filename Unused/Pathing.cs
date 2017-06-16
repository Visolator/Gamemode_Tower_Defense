//Original code made by Port

// ===========================================================================
// When using this code in your own project, please replace the "NG" prefix
// with something else. It needs to be short to improve lookup times in the
// critical path inside the main algorithm, but it also needs to be unique to
// avoid collisions with graph ID spaces or custom versions of the functions.
// ===========================================================================

// ---------------------------------------------------------------------------
// TD_aStarSpatial(start, goal)
//
// Expects 'start' and 'goal' to be valid node references.
// A valid node reference 'a' should have the following defined:
//
//   $TD_NC[a]    - The number of outgoing neighbors this node has on the graph
//   $TD_ND[a, i] - The i'th neighbor, being a valid node reference, i being
//                  any integer from 0 to $TD_NC[a] - 1
//   $TD_PX[a]    - X position in the world
//   $TD_PY[a]    - Y position in the world
//   $TD_PZ[a]    - Z position in the world
//   $TD_D[a, b]  - Euclidean distance (vectorDist) between this node's
//                  position and b's position, b being any valid neighbor
//
// Returns 0 on failure.
//
// Returns the length of the path (start not included) on success.
// $NavPathItem[i] will be set for each i from 0 to (return value) - 1.

function TD_aStarSpatial(%start, %goal)
{
    // Set up the initial state of the priority queue, containing only the
    // start node with the lowest priority
    %frontierSize = 1;
    %frontierItem0 = %start;
    %frontierPriority0 = 0;

    // 0 is assumed for math, but this is to avoid satisfying the $= "" check
    %cost_so_far[%start] = 0;

    // Including goal check inside of the main loop condition
    while (%frontierSize && (%current = %frontierItem0) !$= %goal)
    {
        // Dequeue from the priority queue (backed by a binary heap)
        if (%frontierSize--)
        {
            %frontierItem0 = %frontierItem[%frontierSize];
            %frontierPriority0 = %frontierPriority[%frontierSize];

            %deqCurrent = 0;

            while (%deqCurrent < %frontierSize)
            {
                %deqLargest = %deqCurrent;
                %deqLeft = (2 * %deqCurrent) + 1;
                %deqRight = (2 * %deqCurrent) + 2;

                if (%deqLeft < %frontierSize && %frontierPriority[%deqLeft] < %frontierPriority[%deqLargest])
                    %deqLargest = %deqLeft;

                if (%deqRight < %frontierSize && %frontierPriority[%deqRight] < %frontierPriority[%deqLargest])
                    %deqLargest = %deqRight;

                if (%deqLargest == %deqCurrent)
                    break;

                // There's about half a million variable assignments going
                // on here. How can this be improved?
                %deqTempItem = %frontierItem[%deqLargest];
                %deqTempPriority = %frontierPriority[%deqLargest];
                %frontierItem[%deqLargest] = %frontierItem[%deqCurrent];
                %frontierPriority[%deqLargest] = %frontierPriority[%deqCurrent];
                %frontierItem[%deqCurrent] = %deqTempItem;
                %frontierPriority[%deqCurrent] = %enqTempPriority;

                %deqCurrent = %deqLargest;
            }
        }

        // For calculating the heuristic later
        %x1 = $TD_PX[%current];
        %y1 = $TD_PY[%current];
        %z1 = $TD_PZ[%current];

        for (%i = $TD_NC[%current] - 1; %i >= 0; %i--)
        {
            %new_cost = %cost_so_far[%current] + $TD_D[%current,
                              %next = $TD_NN[%current, %i]];

            if (%cost_so_far[%next] $= "" || %new_cost < %cost_so_far[%next])
            {
                %frontierPriority[%frontierSize] = (%cost_so_far[
                    %frontierItem[%frontierSize] = %next] = %new_cost)
                    + ((%d = $TD_PX[%next] - %x1) < 0 ? -%d : %d)
                    + ((%d = $TD_PY[%next] - %y1) < 0 ? -%d : %d)
                    + ((%d = $TD_PZ[%next] - %z1) < 0 ? -%d : %d);

                // Enqueue on the priority queue (again, binary heap)
                %enqCurrent = %frontierSize;
                %frontierSize++;

                while (%enqCurrent > 0)
                {
                    %enqParent = (%enqCurrent - 1) >> 1;

                    if (%frontierPriority[%enqCurrent] > %frontierPriority[%enqParent])
                        break;

                    // There's about half a million variable assignments going
                    // on here. How can this be improved?
                    %enqTempItem = %frontierItem[%enqParent];
                    %enqTempPriority = %frontierPriority[%enqParent];
                    %frontierItem[%enqParent] = %frontierItem[%enqCurrent];
                    %frontierPriority[%enqParent] = %frontierPriority[%enqCurrent];
                    %frontierItem[%enqCurrent] = %enqTempItem;
                    %frontierPriority[%enqCurrent] = %enqTempPriority;

                    %enqCurrent = %enqParent;
                }

                %came_from[%next] = %current;
            }
        }
    }

    // Trace back the path in reverse
    %size = 0;

    while (%came_from[%goal] !$= "")
    {
        $NavPathItem[%size] = %goal;
        %goal = %came_from[%goal];
        %size++;
    }

    // Reverse the path in-place
    %max = %size >> 1;

    // Temporarily abandoned experiment for slightly faster reversing
    // echo("size = " @ %size);
    // for ((%i = 0) & (%j = %size - 1); %n++ <= 10; %i++ & %j--)
    // {
    //     echo("test" SPC %i SPC %j);
    // }

    for (%i = 0; %i < %max; %i++)
    {
        // echo("ordinary" SPC %i SPC %size - 1 - %i);
        %t = $NavPathItem[%i];
        $NavPathItem[%i] = $NavPathItem[%size - 1 - %i];
        $NavPathItem[%size - 1 - %i] = %t;
    }

    return %size;
}

// Everything down here is for testing
// Comment out the return; if you want to use it
// return;

function tg_reg(%n, %x, %y, %z)
{
    $TD_NC[%n] = 0;
    $TD_PX[%n] = %x;
    $TD_PY[%n] = %y;
    $TD_PZ[%n] = %z;
}

function tg_con(%a, %b)
{
    $TD_NN[%a, $TD_NC[%a]] = %b;
    $TD_NN[%b, $TD_NC[%b]] = %a;
    $TD_NC[%a]++;
    $TD_NC[%b]++;

    $TD_D[%b, %a] = $TD_D[%a, %b] = vectorDist(
        $TD_PX[%a] SPC $TD_PY[%a] SPC $TD_PZ[%a],
        $TD_PX[%b] SPC $TD_PY[%b] SPC $TD_PZ[%b]);
}

function tg_init()
{
    // tg_reg("A", 0, 0);
    // tg_reg("B", 10, -2);
    // tg_reg("C", 10,  5);
    // tg_reg("D", 20);
    //
    // tg_con("A", "B");
    // tg_con("A", "C");
    // tg_con("B", "D");
    // tg_con("C", "D");

    tg_reg("A", 1, 1);
    tg_reg("B", 2, 1);
    tg_reg("C", 2, 2);
    tg_reg("D", 1, 2);
    tg_reg("E", 4, 2);
    tg_reg("F", 3, 0);
    tg_reg("G", -1, 0);

    tg_con("A", "B");
    tg_con("A", "D");
    tg_con("A", "G");
    tg_con("B", "C");
    tg_con("B", "F");
    tg_con("C", "E");
    tg_con("C", "D");
    tg_con("E", "F");
}

tg_init();

function tg_run(%a, %b, %bench)
{
    echo("Path " @ %a @ " -> " @ %b);

    if (%bench)
    {
      %start = getRealTime();

      for (%i = 0; %i < 10000; %i++)
        %size = TD_aStarSpatial(%a, %b);

      %end = getRealTime();
      echo("  Time: " @ (%end - %start) / 10000 SPC "MS");
    }
    else
      %size = TD_aStarSpatial(%a, %b);

    if (%size)
    {
        for (%i = 0; %i < %size; %i++)
        {
            echo("  " @ %i + 1 @ ". " @ $NavPathItem[%i]);
            %dist += $TD_D[%a, $NavPathItem[%i]];
            %a = $NavPathItem[%i];
        }

        echo("  Distance: " @ %dist);
    }
    else
        echo("  Failed");
}