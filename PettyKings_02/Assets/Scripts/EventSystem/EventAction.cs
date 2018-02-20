﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Enum for for describing type of action
public enum ACTIONTYPE
{
    BASE,
    DECISION,
    CAMERAMOVE,
    PAUSE,
    AUDIO
}

// Base Action
[CreateAssetMenu(fileName = "New Action", menuName = "EventActions/BaseAction")]
public class EventAction : ScriptableObject
{

    // properties
    //
    // Type of action
    protected ACTIONTYPE type_;

    // Reference to event this action is in
    protected Event currentEvent_;

    // Bool for ending action
    protected bool actionRunning_ = true;

    // IsBlocking flag
    public bool isBlocking_ = true;

    // Begin method called when action starts
    public virtual void Begin(Event newEvent)
    {
        
        // Set Default values
        type_ = ACTIONTYPE.BASE;
        currentEvent_ = newEvent;
        actionRunning_ = true;
    }


    // End method called when action finishes
    public virtual void End()
    {

    }


    // Called every frame the action is happening
    public virtual bool Update()
    {

        return actionRunning_;
    }

    // Getter for type of event action
    public ACTIONTYPE Type()
    {
        return type_;
    }


    // Print method
    public void Print()
    {

        // Displays information about event to debug log
        Debug.Log(type_);
    }

}

[CreateAssetMenu(fileName = "New Decision Action", menuName = "EventActions/DecisionAction")]
public class DecisionAction : EventAction {

    // All data required for an event

    // Data for displaying initial event i.e. name, description...
    // Main display data
    public EventDisplayData mainDisplay_;

    // Decision display data
    public EventDisplayData[] decisionDisplay_;

    // Modifiers for resources
    public int[] decisionFood_ = new int[2];
    public int[] decisionWood_ = new int[2];
    public int[] decisionMen_ = new int[2];

    // References to event display and resource manager
    private EventDisplay eventDisplay;
    private ResourceManager resourceManager;


    // Called at start of action
    public override void Begin(Event newEvent)
    {
        // Call Base Begin Method
        base.Begin(newEvent);

        // Set action type
        type_ = ACTIONTYPE.DECISION;

        // Create references to eventDisplay and resourceManager
        eventDisplay = EventDisplay.eventDisplay;
        resourceManager = ResourceManager.resourceManager;

        // If there are decisions
        if (decisionDisplay_.Length > 0)
        {
            // Create btnFunction list
            mainDisplay_.btnFunctions_ = new ButtonDel[decisionDisplay_.Length];
            for (int i = 0; i < mainDisplay_.btnFunctions_.Length; i++)
            {

                // Set button functions to DecisionSelected(int choice) method
                mainDisplay_.btnFunctions_[i] = DecisionSelected;
            }

            // Set decision Display button functions
            for (int i = 0; i < decisionDisplay_.Length; i++)
            {

                // Set button functions to ContinuePresseed(int choice) method
                decisionDisplay_[i].btnFunctions_ = new ButtonDel[1] { ContinuePressed };
            }
        }
        else
        {

            // Else no decisions, set button function to ContinuePressed()
            mainDisplay_.btnFunctions_ = new ButtonDel[1] { ContinuePressed };
        }

        // Make event display active
        eventDisplay.gameObject.SetActive(true);

        // Display main
        eventDisplay.Display(mainDisplay_);

    }


    // Called at end of action
    public override void End()
    {

        // Make event display not active
        eventDisplay.gameObject.SetActive(false);
    }


    // Called every frame action is active
    public override bool Update()
    {

        // QUICK FIX FOR WEIRD BUG (SHOULD REMOVE THIS CODE WHEN BUG IS FIXED)
        if (actionRunning_)
        {
            eventDisplay.gameObject.SetActive(true);
        }
        else
        {
            eventDisplay.gameObject.SetActive(false);
        }

        return actionRunning_;
    }


    // Method for decision events
    public void DecisionSelected(int choice)
    {

        // If event Display exists
        if (eventDisplay != null)
        {
            // Display choice made
            eventDisplay.Display(decisionDisplay_[choice]);

            // Update resources from decision made
            resourceManager.UpdateResources(GetDecisionResources(choice));
        }

    }


    // Method for continuing to next action
    public void ContinuePressed(int choice)
    {
        // Set actionRunning_ to false to end the action
        actionRunning_ = false;
    }


    // Returns Resources from a choice
    public int[] GetDecisionResources(int choice)
    {
        int[] resources = new int[3];
        resources[0] = decisionFood_[choice];
        resources[1] = decisionWood_[choice];
        resources[2] = decisionMen_[choice];
        return resources;
    }

}


[CreateAssetMenu(fileName = "New Camera Move", menuName = "EventActions/CameraMove")]
public class CameraMoveAction : EventAction
{

    // properties
    public CameraGoto[] cameraGoto_;
    
    private CameraController cameraController;

    // Begin method called when action starts
    public override void Begin(Event newEvent)
    {
        base.Begin(newEvent);

        type_ = ACTIONTYPE.CAMERAMOVE;

        cameraController = Camera.main.GetComponent<CameraController>();

        for (int i = 0; i < cameraGoto_.Length; i++)
        {
            cameraController.AddGotoPosition(cameraGoto_[i]);
        }
    }


    // End method called when action finishes
    public override void End()
    {

    }


    // Called every frame the action is happening
    public override bool Update()
    {


        if (cameraController.FinishedMove())
        {
            actionRunning_ = false;
        }

        return actionRunning_;
    }
}


[CreateAssetMenu(fileName = "New Pause Action", menuName = "EventActions/Pause")]
public class PauseAction : EventAction
{
    // properties
    public float pauseLength_;

    private Timer timer_;

    // Begin method called when action starts
    public override void Begin(Event newEvent)
    {
        base.Begin(newEvent);

        type_ = ACTIONTYPE.PAUSE;

        timer_ = new Timer();
        timer_.SetTimer(pauseLength_);
    }


    // End method called when action finishes
    public override void End()
    {

    }


    // Called every frame the action is happening
    public override bool Update()
    {


        if (timer_.UpdateTimer())
        {
            actionRunning_ = false;
        }

        return actionRunning_;
    }
}

[CreateAssetMenu(fileName = "New Audio Action", menuName = "EventActions/Audio")]
public class AudioAction : EventAction
{

    // properties
    public AudioClip audioClip_;
    [Range(0.0f, 1.0f)]
    public float volumeScale_;

    private AudioSource audioSource_;

    // Begin method called when action starts
    public override void Begin(Event newEvent)
    {
        base.Begin(newEvent);

        type_ = ACTIONTYPE.AUDIO;

        audioSource_ = EventController.eventController.gameObject.GetComponent<AudioSource>();

        audioSource_.PlayOneShot(audioClip_, volumeScale_);
    }


    // End method called when action finishes
    public override void End()
    {

    }


    // Called every frame the action is happening
    public override bool Update()
    {


        if (!audioSource_.isPlaying)
        {
            actionRunning_ = false;
        }

        return actionRunning_;
    }
}

