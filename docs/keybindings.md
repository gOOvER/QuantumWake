# Star Citizen keybindings, Alpha 4.10.193.11644

Every action the game can bind - 1103 in 50 groups - read from
`Data\Libs\Config\defaultProfile.xml` in this install's archive and labelled from its
own strings, with the default input per device kind and how the action fires.
A dash is bindable but unbound by default. Generated on 2026-09-17 by
`dotnet run --project src\Quantumwake.Cli -c Release -- --keys=md > docs\keybindings.md`;
regenerate after a patch and diff. `--keys` alone prints the same with this install's
own bindings beside each action.

## Vehicles - Seats and Operator Modes

`seat_general` · 27 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Emergency Exit Seat | `v_emergency_exit` | u+lshift | — | — | — | tap |
| Eject | `v_eject` | ralt+y | — | — | — | press |
| Look behind | `v_view_look_behind` | comma | — | shoulderl+a | — | delayed_hold_no_retrigger |
| Toggle Mining Operator Mode | `v_toggle_mining_mode` | m | — | — | — | press |
| Toggle Salvage Operator Mode | `v_toggle_salvage_mode` | m | — | — | — | press |
| Toggle Refuel Operator Mode | `v_toggle_refuel_mode` | m | — | — | — | press |
| Toggle Scanning Operator Mode | `v_toggle_scan_mode` | v | — | dpad_right | — | press |
| Toggle Quantum Operator Mode | `v_toggle_quantum_mode` | — | — | — | — | press |
| Toggle Missile Operator Mode | `v_toggle_missile_mode` | — | — | — | — | press |
| Toggle Guns Operator Mode | `v_toggle_guns_mode` | — | — | — | — | press |
| Toggle Flight Operator Mode | `v_toggle_flight_mode` | — | — | — | — | press |
| Set Mining Operator Mode | `v_set_mining_mode` | — | — | — | — | press |
| Set Salvage Operator Mode | `v_set_salvage_mode` | — | — | — | — | press |
| Set Refuel Operator Mode | `v_set_refuel_mode` | — | — | — | — | press |
| Set Scanning Operator Mode | `v_set_scan_mode` | — | — | — | — | press |
| Set Quantum Operator Mode | `v_set_quantum_mode` | — | — | — | — | press |
| Set Missile Operator Mode | `v_set_missile_mode` | — | — | — | — | press |
| Set Guns Operator Mode | `v_set_guns_mode` | — | — | — | — | press |
| Set Flight Operator Mode | `v_set_flight_mode` | — | — | — | — | press |
| Enter Remote Turret 1 | `v_enter_remote_turret_1` | — | — | — | — | press |
| Enter Remote Turret 2 | `v_enter_remote_turret_2` | — | — | — | — | press |
| Enter Remote Turret 3 | `v_enter_remote_turret_3` | — | — | — | — | press |
| Next Operator Mode | `v_operator_mode_cycle_forward` | mouse3 | — | — | — | tap |
| Previous Operator Mode | `v_operator_mode_cycle_back` | — | — | — | — | tap |
| Light Amplification Toggle | `v_light_amplification_toggle` | ralt+l | — | — | — | tap |
| Light Amplification On | `v_light_amplification_on` | — | — | — | — | tap |
| Light Amplification Off | `v_light_amplification_off` | — | — | — | — | tap |

## Vehicles - Cockpit · FLIGHT

`spaceship_general` · 19 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Self Destruct | `v_self_destruct` | backspace | — | — | — | delayed_press_medium |
| Increase Cooler Rate | `v_cooler_throttle_up` | — | — | — | — | — |
| Decrease Cooler Rate | `v_cooler_throttle_down` | — | — | — | — | — |
| spectate_enterpuremode | `spectate_enterpuremode` | — | — | — | — | delayed_press |
| Flight / Systems Ready | `v_flightready` | ralt+r | — | — | — | press |
| Open/Close Doors (Toggle) | `v_toggle_all_doors` | — | — | — | — | press |
| Open All Doors | `v_open_all_doors` | — | — | — | — | press |
| Close All Doors | `v_close_all_doors` | — | — | — | — | press |
| Lock/Unlock Doors (Toggle) | `v_toggle_all_doorlocks` | — | — | — | — | press |
| Lock All Doors | `v_lock_all_doors` | — | — | — | — | press |
| Unlock All Doors | `v_unlock_all_doors` | — | — | — | — | press |
| Port Lock Toggle All | `v_toggle_all_portlocks` | ralt+K | — | — | — | press |
| Port Lock All | `v_lock_all_ports` | — | — | — | — | press |
| Port Unlock All | `v_unlock_all_ports` | — | — | — | — | press |
| pc_conversation_option1 | `pc_conversation_option1` | 1 | — | — | — | all |
| pc_conversation_option2 | `pc_conversation_option2` | 2 | — | — | — | all |
| pc_conversation_option3 | `pc_conversation_option3` | 3 | — | — | — | all |
| pc_conversation_option4 | `pc_conversation_option4` | 4 | — | — | — | all |
| pc_conversation_option5 | `pc_conversation_option5` | 5 | — | — | — | all |

## Vehicles - Multi Function Displays (MFDs)

`vehicle_mfd` · 55 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| MFD - Cycle Page - Forwards (Short Press) | `v_mfd_interact_cycle_forwards_short` | lalt+e | — | — | — | tap |
| MFD - Cycle Page - Forwards (Long Press) | `v_mfd_interact_cycle_forwards_long` | — | — | — | — | delayed_press |
| MFD - Cycle Page - Backwards (Short Press) | `v_mfd_interact_cycle_backwards_short` | lalt+q | — | — | — | tap |
| MFD - Cycle Page - Backwards (Long Press) | `v_mfd_interact_cycle_backwards_long` | — | — | — | — | delayed_press |
| MFD - Movement - Up (Short Press) | `v_mfd_movement_up_short` | — | — | — | — | tap |
| MFD - Movement - Up (Long Press) | `v_mfd_movement_up_long` | — | — | — | — | delayed_press |
| MFD - Movement - Down (Short Press) | `v_mfd_movement_down_short` | — | — | — | — | tap |
| MFD - Movement - Down (Long Press) | `v_mfd_movement_down_long` | — | — | — | — | delayed_press |
| MFD - Movement - Left (Short Press) | `v_mfd_movement_left_short` | — | — | — | — | tap |
| MFD - Movement - Left (Long Press) | `v_mfd_movement_left_long` | — | — | — | — | delayed_press |
| MFD - Movement - Right (Short Press) | `v_mfd_movement_right_short` | — | — | — | — | tap |
| MFD - Movement - Right (Long Press) | `v_mfd_movement_right_long` | — | — | — | — | delayed_press |
| MFD - Select - Primary (Short Press) | `v_mfd_soft_select_mfd_primary_short` | — | — | — | — | tap |
| MFD - Select - Primary (Long Press) | `v_mfd_soft_select_mfd_primary_long` | — | — | — | — | delayed_press |
| MFD - Select - Left Cast (Short Press) | `v_mfd_soft_select_cast_left_short` | — | — | — | — | tap |
| MFD - Select - Left Cast (Long Press) | `v_mfd_soft_select_cast_left_long` | — | — | — | — | delayed_press |
| MFD - Select - Right Cast (Short Press) | `v_mfd_soft_select_cast_right_short` | — | — | — | — | tap |
| MFD - Select - Right Cast (Long Press) | `v_mfd_soft_select_cast_right_long` | — | — | — | — | delayed_press |
| MFD - Select - MFD 1 (Short Press) | `v_mfd_soft_select_mfd_1_short` | — | — | — | — | tap |
| MFD - Select - MFD 1 (Long Press) | `v_mfd_soft_select_mfd_1_long` | — | — | — | — | delayed_press |
| MFD - Select - MFD 2 (Short Press) | `v_mfd_soft_select_mfd_2_short` | — | — | — | — | tap |
| MFD - Select - MFD 2 (Long Press) | `v_mfd_soft_select_mfd_2_long` | — | — | — | — | delayed_press |
| MFD - Select - MFD 3 (Short Press) | `v_mfd_soft_select_mfd_3_short` | — | — | — | — | tap |
| MFD - Select - MFD 3 (Long Press) | `v_mfd_soft_select_mfd_3_long` | — | — | — | — | delayed_press |
| MFD - Select - MFD 4 (Short Press) | `v_mfd_soft_select_mfd_4_short` | — | — | — | — | tap |
| MFD - Select - MFD 4 (Long Press) | `v_mfd_soft_select_mfd_4_long` | — | — | — | — | delayed_press |
| MFD - Select - MFD 5 (Short Press) | `v_mfd_soft_select_mfd_5_short` | — | — | — | — | tap |
| MFD - Select - MFD 5 (Long Press) | `v_mfd_soft_select_mfd_5_long` | — | — | — | — | delayed_press |
| MFD - Select - MFD 6 (Short Press) | `v_mfd_soft_select_mfd_6_short` | — | — | — | — | tap |
| MFD - Select - MFD 6 (Long Press) | `v_mfd_soft_select_mfd_6_long` | — | — | — | — | delayed_press |
| MFD - Select - MFD 7 (Short Press) | `v_mfd_soft_select_mfd_7_short` | — | — | — | — | tap |
| MFD - Select - MFD 7 (Long Press) | `v_mfd_soft_select_mfd_7_long` | — | — | — | — | delayed_press |
| MFD - Select - MFD 8 (Short Press) | `v_mfd_soft_select_mfd_8_short` | — | — | — | — | tap |
| MFD - Select - MFD 8 (Long Press) | `v_mfd_soft_select_mfd_8_long` | — | — | — | — | delayed_press |
| MFD - Select - MFD 9 (Short Press) | `v_mfd_soft_select_mfd_9_short` | — | — | — | — | tap |
| MFD - Select - MFD 9 (Long Press) | `v_mfd_soft_select_mfd_9_long` | — | — | — | — | delayed_press |
| MFD - Select - MFD 10 (Short Press) | `v_mfd_soft_select_mfd_10_short` | — | — | — | — | tap |
| MFD - Select - MDF 10 (Long Press) | `v_mfd_soft_select_mfd_10_long` | — | — | — | — | delayed_press |
| MFD - Quick Action - Self Repair All | `v_mfd_quick_action_repair_all` | — | — | — | — | tap |
| MFD - Set Page - Self Status (Short Press) | `v_mfd_select_view_self_status_short` | — | — | — | — | tap |
| MFD - Set Page - Self Status (Long Press) | `v_mfd_select_view_self_status_long` | — | — | — | — | delayed_press |
| MFD - Set Page - Target Status (Short Press) | `v_mfd_select_view_target_status_short` | — | — | — | — | tap |
| MFD - Set Page - Target Status (Long Press) | `v_mfd_select_view_target_status_long` | — | — | — | — | delayed_press |
| MFD - Set Page - Scanning (Short Press) | `v_mfd_select_view_scanning_short` | — | — | — | — | tap |
| MFD - Set Page - Scanning (Long Press) | `v_mfd_select_view_scanning_long` | — | — | — | — | delayed_press |
| MFD - Set Page - Vehicle Configuration (Short Press) | `v_mfd_select_view_configuration_short` | — | — | — | — | tap |
| MFD - Set Page - Vehicle Configuration (Long Press) | `v_mfd_select_view_configuration_long` | — | — | — | — | delayed_press |
| MFD - Set Page - Communications (Short Press) | `v_mfd_select_view_comms_short` | — | — | — | — | tap |
| MFD - Set Page - Communications (Long Press) | `v_mfd_select_view_comms_long` | — | — | — | — | delayed_press |
| MFD - Set Page - IFCS (Short Press) | `v_mfd_select_view_ifcs_short` | — | — | — | — | tap |
| MFD - Set Page - IFCS (Long Press) | `v_mfd_select_view_ifcs_long` | — | — | — | — | delayed_press |
| MFD - Set Page - Diagnostics (Short Press) | `v_mfd_select_view_diagnostics_short` | — | — | — | — | tap |
| MFD - Set Page - Diagnostics (Long Press) | `v_mfd_select_view_diagnostics_long` | — | — | — | — | delayed_press |
| MFD - Set Page - Resource Network (Short Press) | `v_mfd_select_view_resource_network_short` | — | — | — | — | tap |
| MFD - Set Page - Resource Network (Long Press) | `v_mfd_select_view_resource_network_long` | — | — | — | — | delayed_press |

## Vehicles - View · FLIGHT

`spaceship_view` · 28 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Look left | `v_view_yaw_left` | — | — | — | — | — |
| Look right | `v_view_yaw_right` | — | — | — | — | — |
| Look left / right | `v_view_yaw` | — | — | thumbrx | x | — |
| Look left / right | `v_view_yaw_mouse` | — | maxis_x | — | — | — |
| v_view_yaw_absolute | `v_view_yaw_absolute` | HMD_Yaw | — | — | — | — |
| Look up | `v_view_pitch_up` | — | — | — | — | — |
| Look down | `v_view_pitch_down` | — | — | — | — | — |
| Look up / down | `v_view_pitch` | — | — | thumbry | y | — |
| Look up / down | `v_view_pitch_mouse` | — | maxis_y | — | — | — |
| v_view_pitch_absolute | `v_view_pitch_absolute` | HMD_Pitch | — | — | — | — |
| v_view_roll_absolute | `v_view_roll_absolute` | HMD_Roll | — | — | — | — |
| Cycle camera view | `v_view_cycle_fwd` | f4 | — | shoulderl+y | — | tap |
| v_view_cycle_internal_fwd | `v_view_cycle_internal_fwd` | — | — | — | — | — |
| v_view_option | `v_view_option` | — | — | — | — | — |
| Cycle camera orbit mode | `v_view_mode` | — | — | — | — | — |
| Zoom in (3rd person view) | `v_view_zoom_in` | — | mwheel_up | — | — | — |
| Zoom out (3rd person view) | `v_view_zoom_out` | — | mwheel_down | — | — | — |
| v_view_interact | `v_view_interact` | f | — | a | — | — |
| Freelook (Hold) | `v_view_freelook_mode` | z | — | — | — | hold |
| Dynamic Zoom In and Out (rel.) | `v_view_dynamic_zoom_rel` | — | — | — | — | — |
| Dynamic Zoom In (rel.) | `v_view_dynamic_zoom_rel_in` | — | — | — | — | all |
| Dynamic Zoom Out (rel.) | `v_view_dynamic_zoom_rel_out` | — | — | — | — | all |
| Dynamic Zoom In and Out (abs.) | `v_view_dynamic_zoom_abs` | — | — | — | — | — |
| Dynamic Zoom Toggle (abs.) | `v_view_dynamic_zoom_abs_toggle` | — | — | — | — | smart_toggle |
| Precision Targeting - Hold | `v_ads_hold` | — | — | — | — | hold |
| Precision Targeting - Toggle On / Off | `v_ads_toggle` | mouse2 | — | — | — | tap |
| Precision Targeting - Maximum Zoom (hold) | `v_ads_stable_max_zoom_hold` | mouse2 | — | — | — | delayed_hold |
| Precision Targeting - Toggle Camera Tracking | `v_ads_cycle_tracking` | ralt+mouse2 | — | — | — | tap |

## Flight - Movement · FLIGHT

`spaceship_movement` · 103 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Pitch up | `v_pitch_up` | down | — | — | — | — |
| Pitch down | `v_pitch_down` | up | — | — | — | — |
| Pitch | `v_pitch` | — | — | thumbry | y | — |
| Pitch | `v_pitch_mouse` | — | maxis_y | — | — | — |
| Yaw left | `v_yaw_left` | left | — | — | — | — |
| Yaw right | `v_yaw_right` | right | — | — | — | — |
| Yaw | `v_yaw` | — | — | thumblx | x | — |
| Yaw | `v_yaw_mouse` | — | maxis_x | — | — | — |
| Roll left | `v_roll_left` | q | — | — | — | — |
| Roll right | `v_roll_right` | e | — | — | — | — |
| Roll | `v_roll` | — | — | thumbrx | rotz | — |
| Roll | `v_roll_mouse` | — | — | — | — | — |
| Cycle mouse mode (VJoy / Relative) | `v_toggle_relative_mouse_mode` | — | — | — | — | smart_toggle |
| Swap Yaw / Roll (Toggle) | `v_toggle_yaw_roll_swap` | — | — | thumbr | — | smart_toggle |
| Strafe up (abs.) | `v_strafe_up` | space | — | — | button9 | — |
| Strafe down (abs.) | `v_strafe_down` | lctrl | — | — | button10 | — |
| Strafe up / down (abs.) | `v_strafe_vertical` | — | — | shoulderl+thumbly | — | — |
| Strafe left (abs.) | `v_strafe_left` | a | — | — | hat1_left | — |
| Strafe right (abs.) | `v_strafe_right` | d | — | — | hat1_right | — |
| Strafe left / right (abs.) | `v_strafe_lateral` | — | — | shoulderl+thumblx | — | — |
| Throttle - Increase | `v_strafe_forward` | w | — | — | — | — |
| Throttle - Decrease | `v_strafe_back` | s | — | — | — | — |
| Throttle - Forward / Back | `v_strafe_longitudinal` | — | — | thumbly | — | — |
| Throttle - Forward / Back Invert | `v_strafe_longitudinal_invert` | — | — | — | — | — |
| Throttle - Cruise Mode - Toggle | `v_ifcs_throttle_swap_mode` | lalt+c | — | — | — | — |
| Throttle - Cruise Mode - Enable | `v_ifcs_throttle_set_sticky` | — | — | — | — | — |
| Throttle - Cruise Mode - Disable | `v_ifcs_throttle_set_normal` | — | — | — | — | — |
| Throttle - Trim - Set (Long Press) | `v_strafe_trim_set_long` | — | — | — | — | delayed_press |
| Throttle - Trim - Set (Short Press) | `v_strafe_trim_set_short` | — | — | — | — | tap |
| Throttle - Trim - Set To 100% (Long Press) | `v_strafe_trim_set_100_long` | — | — | — | — | delayed_press |
| Throttle - Trim - Set To 100% (Short Press) | `v_strafe_trim_set_100_short` | — | — | — | — | tap |
| Throttle - Trim - Set To 50% (Long Press) | `v_strafe_trim_set_50_long` | — | — | — | — | delayed_press |
| Throttle - Trim - Set To 50% (Short Press) | `v_strafe_trim_set_50_short` | — | — | — | — | tap |
| Throttle - Trim - Release (Long Press) | `v_strafe_trim_reset_long` | x | — | — | — | delayed_press |
| Throttle - Trim - Release (Short Press) | `v_strafe_trim_reset_short` | x | — | — | — | tap |
| Enable / Disable decoupled mode | `v_ifcs_vector_decoupling_toggle` | c | — | b | — | tap |
| Enable decoupled mode | `v_ifcs_vector_decoupling_on` | — | — | — | — | press |
| Disable decoupled mode | `v_ifcs_vector_decoupling_off` | — | — | — | — | press |
| Boost | `v_afterburner` | lshift | — | thumbl | button8 | all |
| Speed Limiter - Increase (hold) | `v_ifcs_speed_limiter_up` | — | — | — | — | — |
| Speed Limiter - Decrease (hold) | `v_ifcs_speed_limiter_down` | — | — | — | — | — |
| Speed Limiter - Step Up (tap) | `v_ifcs_speed_limiter_increment` | lalt+mwheel_up | — | — | — | press |
| Speed Limiter - Step Down (tap) | `v_ifcs_speed_limiter_decrement` | lalt+mwheel_down | — | — | — | press |
| Speed Limiter (rel) | `v_ifcs_speed_limiter_rel` | — | — | — | — | — |
| Speed Limiter (abs) | `v_ifcs_speed_limiter_abs` | — | — | — | — | — |
| Speed Limiter - Enable / Disable | `v_ifcs_speed_limiter_toggle` | — | — | — | — | tap |
| Speed Limiter - Enable | `v_ifcs_speed_limiter_on` | — | — | — | — | press |
| Speed Limiter - Disable | `v_ifcs_speed_limiter_off` | — | — | — | — | press |
| Acceleration Limiter - Increase (hold) | `v_accel_range_up` | — | — | — | — | — |
| Acceleration Limiter - Decrease (hold) | `v_accel_range_down` | — | — | — | — | — |
| Acceleration Limiter - Step Up (tap) | `v_accel_range_increment` | ralt+mwheel_up | — | — | — | press |
| Acceleration Limiter - Step Down (tap) | `v_accel_range_decrement` | ralt+mwheel_down | — | — | — | press |
| Acceleration Limiter (rel) | `v_accel_range_rel` | — | — | — | — | — |
| Acceleration Limiter (abs) | `v_accel_range_abs` | — | — | — | — | — |
| Spacebrake | `v_space_brake` | x | — | — | — | — |
| Lock Pitch / Yaw Movement (Toggle / Hold) | `v_lock_rotation` | rshift | — | — | — | smart_toggle |
| G-Force safety on | `v_ifcs_gsafe_on` | — | — | — | — | press |
| G-Force safety off | `v_ifcs_gsafe_off` | — | — | — | — | press |
| G-Force Safety On/Off (Toggle / Hold) | `v_ifcs_toggle_gforce_safety` | — | — | — | — | tap |
| E.S.P. - Toggle On / Off (Press) | `v_ifcs_toggle_esp` | — | — | — | — | smart_toggle |
| E.S.P. - Enable Temporarily (Hold) | `v_ifcs_esp_hold` | — | — | — | — | hold |
| Landing System (Toggle) | `v_toggle_landing_system` | n | — | dpad_down | button12 | tap |
| Landing System (Deploy) | `v_deploy_landing_system` | — | — | — | — | press |
| Landing System (Retract) | `v_retract_landing_system` | — | — | — | — | press |
| Toggle VTOL | `v_vtol_toggle` | k | — | — | — | tap |
| Enable VTOL | `v_vtol_on` | — | — | — | — | press |
| Disable VTOL | `v_vtol_off` | — | — | — | — | press |
| Expand Configuration | `v_transform_deploy` | — | — | — | — | press |
| Retract Configuration | `v_transform_retract` | — | — | — | — | press |
| Cycle Configuration | `v_transform_cycle` | lalt+k | — | — | — | tap |
| Autoland | `v_autoland` | n | — | dpad_down | button12 | delayed_press |
| Request Landing | `v_atc_request` | lalt+n | — | — | — | tap |
| Request Cargo Loading | `v_atc_loading_area_request` | ralt+n | — | — | — | tap |
| Cycle Master Mode (Short Press) | `v_master_mode_cycle` | — | — | — | — | tap |
| Cycle Master Mode (Long Press) | `v_master_mode_cycle_long` | b | — | — | — | delayed_press |
| Set Master Mode to Nav | `v_master_mode_set_nav` | — | — | — | — | press |
| Set Master Mode to SCM | `v_master_mode_set_scm` | — | — | — | — | press |
| Jump Drive - Request Jump | `v_toggle_jump_request` | — | mouse1 | — | — | tap |
| IFCS - Gravity Compensation - Toggle | `v_ifcs_gravity_compensation_toggle` | — | — | — | — | tap |
| IFCS - Gravity Compensation - Enable | `v_ifcs_gravity_compensation_on` | — | — | — | — | press |
| IFCS - Gravity Compensation - Disable | `v_ifcs_gravity_compensation_off` | — | — | — | — | press |
| IFCS - Wind Compensation - Toggle | `v_ifcs_wind_compensation_toggle` | — | — | — | — | tap |
| IFCS - Wind Compensation - Enable | `v_ifcs_wind_compensation_on` | — | — | — | — | press |
| IFCS - Wind Compensation - Disable | `v_ifcs_wind_compensation_off` | — | — | — | — | press |
| Automatic Precision Mode - Toggle | `v_auto_precision_mode_toggle` | — | — | — | — | tap |
| Automatic Precision Mode - Enable | `v_auto_precision_mode_on` | — | — | — | — | press |
| Automatic Precision Mode - Disable | `v_auto_precision_mode_off` | — | — | — | — | press |
| IFCS - Proximity Assist - Toggle | `v_ifcs_proximity_assist_toggle` | — | — | — | — | tap |
| IFCS - Proximity Assist - Enable | `v_ifcs_proximity_assist_on` | — | — | — | — | press |
| IFCS - Proximity Assist - Disable | `v_ifcs_proximity_assist_off` | — | — | — | — | press |
| IFCS - Stability - Toggle | `v_ifcs_stability_toggle` | — | — | — | — | tap |
| IFCS - Stability - Enable | `v_ifcs_stability_on` | — | — | — | — | press |
| IFCS - Stability - Disable | `v_ifcs_stability_off` | — | — | — | — | press |
| IFCS Command Behaviour Toggle | `v_ifcs_command_toggle` | — | — | — | — | tap |
| IFCS Command Behaviour On | `v_ifcs_command_on` | — | — | — | — | press |
| IFCS Command Behaviour Off | `v_ifcs_command_off` | — | — | — | — | press |
| IFCS - Core - Toggle On / Off | `v_ifcs_core_toggle` | — | — | — | — | tap |
| IFCS - Core - Enable | `v_ifcs_core_on` | — | — | — | — | press |
| IFCS - Core - Disable | `v_ifcs_core_off` | — | — | — | — | press |
| Reset Flight Accelerometer | `v_ifcs_reset_gmeter_max` | ralt+c | — | — | — | tap |
| Advanced HUD - Toggle | `v_flight_advanced_hud_toggle` | — | — | — | — | tap |
| Advanced HUD - Enable | `v_flight_advanced_hud_on` | — | — | — | — | press |
| Advanced HUD - Disable | `v_flight_advanced_hud_off` | — | — | — | — | press |

## Flight - Quantum Travel

`spaceship_quantum` · 1 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Engage Quantum Drive (Hold) | `v_toggle_qdrive_engagement` | mouse1 | — | triggerr_btn | button1 | delayed_press |

## Flight - Docking

`spaceship_docking` · 2 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Docking\n(Initiate) | `v_toggle_docking_request` | ralt+n | — | — | — | tap |
| Toggle Docking Camera | `v_dock_toggle_view` | 0 | — | — | — | delayed_press |

## Vehicles - Targeting · FLIGHT

`spaceship_targeting` · 25 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Auto Targeting - Toggle On/Off (Long Press) | `v_auto_targeting_toggle_long` | t | — | — | — | delayed_press |
| Auto Targeting - Toggle On/Off (Short Press) | `v_auto_targeting_toggle_short` | — | — | — | — | tap |
| Auto Targeting - Toggle On (Short Press) | `v_auto_targeting_enable_short` | — | — | — | — | tap |
| Auto Targeting - Toggle On (Long Press) | `v_auto_targeting_enable_long` | — | — | — | — | delayed_press |
| Auto Targeting - Toggle Off (Short Press) | `v_auto_targeting_disable_short` | — | — | — | — | tap |
| Auto Targeting - Toggle Off (Long Press) | `v_auto_targeting_disable_long` | — | — | — | — | delayed_press |
| Pin Index 1 - Lock / Unlock Pinned Target | `v_target_toggle_lock_index_1` | 1 | — | — | — | tap |
| Pin Index 2 - Lock / Unlock Pinned Target | `v_target_toggle_lock_index_2` | 2 | — | — | — | tap |
| Pin Index 3 - Lock / Unlock Pinned Target | `v_target_toggle_lock_index_3` | 3 | — | — | — | tap |
| Pin Index 1 - Pin / Unpin Selected Target | `v_target_toggle_pin_index_1` | lalt+1 | — | — | — | tap |
| Pin Index 2 - Pin / Unpin Selected Target | `v_target_toggle_pin_index_2` | lalt+2 | — | — | — | tap |
| Pin Index 3 - Pin / Unpin Selected Target | `v_target_toggle_pin_index_3` | lalt+3 | — | — | — | tap |
| Pin Index 1 - Pin / Unpin Selected Target (Hold) | `v_target_toggle_pin_index_1_hold` | — | — | — | — | delayed_press |
| Pin Index 2 - Pin / Unpin Selected Target (Hold) | `v_target_toggle_pin_index_2_hold` | — | — | — | — | delayed_press |
| Pin Index 3 - Pin / Unpin Selected Target (Hold) | `v_target_toggle_pin_index_3_hold` | — | — | — | — | delayed_press |
| Pin Selected Target | `v_target_pin_selected` | — | — | — | — | tap |
| Unpin Selected Target | `v_target_unpin_selected` | — | — | — | — | tap |
| Pin Selected Target (Hold) | `v_target_pin_selected_hold` | — | — | — | — | delayed_press |
| Unpin Selected Target (Hold) | `v_target_unpin_selected_hold` | — | — | — | — | delayed_press |
| Remove All Pinned Targets | `v_target_remove_all_pins` | 0 | — | — | — | tap |
| Lock Selected Target | `v_target_lock_selected` | — | — | — | — | tap |
| Unlock Current Target | `v_target_unlock` | lalt+t | — | — | — | tap |
| Enable / Disable Look Ahead | `v_look_ahead_enable` | lalt+l | — | — | — | smart_toggle |
| Enable / Disable Target Padlock (Toggle, Hold) | `v_look_ahead_start_target_tracking` | — | — | — | — | smart_toggle |
| Auto Zoom On Selected Target On / Off (Toggle, Hold) | `v_target_tracking_auto_zoom` | — | — | — | — | smart_toggle |

## Vehicles - Target Cycling · FLIGHT

`spaceship_targeting_advanced` · 22 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Lock Target Under Reticle | `v_target_under_reticle` | — | — | — | — | tap |
| Cycle Lock - In View - Back | `v_target_cycle_in_view_back` | — | — | — | — | tap |
| Cycle Lock - In View - Forward | `v_target_cycle_in_view_fwd` | t | — | — | — | tap |
| Cycle Lock - In View - Under Reticle | `v_target_cycle_in_view_reset` | t | — | — | — | tap |
| Cycle Lock - Pinned - Back | `v_target_cycle_pinned_back` | — | — | — | — | tap |
| Cycle Lock - Pinned - Forward | `v_target_cycle_pinned_fwd` | — | — | — | — | tap |
| Cycle Lock - Pinned - Reset to First | `v_target_cycle_pinned_reset` | — | — | — | — | tap |
| Cycle Lock - Attackers - Back | `v_target_cycle_attacker_back` | — | — | — | — | tap |
| Cycle Lock - Attackers - Forward | `v_target_cycle_attacker_fwd` | 4 | — | — | — | tap |
| Cycle Lock - Attackers - Reset to Closest | `v_target_cycle_attacker_reset` | 4 | — | — | — | tap |
| Cycle Lock - Hostiles - Back | `v_target_cycle_hostile_back` | — | — | — | — | tap |
| Cycle Lock - Hostiles - Forward | `v_target_cycle_hostile_fwd` | 5 | — | — | — | tap |
| Cycle Lock - Hostiles - Reset to Closest | `v_target_cycle_hostile_reset` | 5 | — | — | — | tap |
| Cycle Lock - Friendlies - Back | `v_target_cycle_friendly_back` | — | — | — | — | tap |
| Cycle Lock - Friendlies - Forward | `v_target_cycle_friendly_fwd` | 6 | — | — | — | tap |
| Cycle Lock - Friendlies - Reset to Closest | `v_target_cycle_friendly_reset` | 6 | — | — | — | tap |
| Cycle Lock - All - Back | `v_target_cycle_all_back` | — | — | — | — | tap |
| Cycle Lock - All - Forward | `v_target_cycle_all_fwd` | 7 | — | — | — | tap |
| Cycle Lock - All - Reset to Closest | `v_target_cycle_all_reset` | 7 | — | — | — | tap |
| Cycle Lock - Sub-Target - Back | `v_target_cycle_subitem_back` | — | — | — | — | tap |
| Cycle Lock - Sub-Target - Forward | `v_target_cycle_subitem_fwd` | r | — | — | — | tap |
| Cycle Lock - Sub-Target - Reset to Main Target | `v_target_cycle_subitem_reset` | lalt+r | — | — | — | tap |

## Flight - Target Hailing · FLIGHT

`spaceship_target_hailing` · 1 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Hail Target | `v_target_hail` | 9 | — | — | — | tap |

## Flight - Radar · FLIGHT

`spaceship_radar` · 1 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Activate Ping (Hold & Release) | `v_invoke_ping` | tab | — | y | — | hold |

## Vehicles - Scanning · FLIGHT

`spaceship_scanning` · 11 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Activate Scanning | `v_scanning_trigger_scan` | — | mouse1 | — | — | hold |
| Increase Scanning Angle | `v_inc_scan_focus_level` | — | mwheel_up | — | button3 | press |
| Decrease Scanning Angle | `v_dec_scan_focus_level` | — | mwheel_down | — | button4 | press |
| v_ui_prev_scan_tab | `v_ui_prev_scan_tab` | left | — | — | — | tap |
| v_ui_next_scan_tab | `v_ui_next_scan_tab` | right | — | — | — | tap |
| v_ui_prev_scan_page | `v_ui_prev_scan_page` | up | — | — | — | tap |
| v_ui_next_scan_page | `v_ui_next_scan_page` | down | — | — | — | tap |
| v_ui_prev_contact_page | `v_ui_prev_contact_page` | rctrl+left | — | — | — | tap |
| v_ui_next_contact_page | `v_ui_next_contact_page` | rctrl+right | — | — | — | tap |
| v_ui_prev_contact | `v_ui_prev_contact` | rctrl+up | — | — | — | tap |
| v_ui_next_contact | `v_ui_next_contact` | rctrl+down | — | — | — | tap |

## Vehicles - Mining · FLIGHT

`spaceship_mining` · 10 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Fire Mining Laser (Toggle) | `v_toggle_mining_laser_fire` | mouse1 | — | — | button1 | smart_toggle |
| Switch Mining Laser (Toggle) | `v_toggle_mining_laser_type` | lalt+mouse1 | — | — | button2 | press |
| Increase Mining Laser Power | `v_increase_mining_throttle` | maxis_z | — | — | button3 | press |
| Decrease Mining Laser Power | `v_decrease_mining_throttle` | — | — | — | button4 | press |
| Increase / Decrease Mining Laser Power | `v_mining_throttle` | — | — | — | — | — |
| Activate Mining Module (Slot 1) | `v_mining_use_consumable1` | lalt+1 | — | — | — | press |
| Activate Mining Module (Slot 2) | `v_mining_use_consumable2` | lalt+2 | — | — | — | press |
| Activate Mining Module (Slot 3) | `v_mining_use_consumable3` | lalt+3 | — | — | — | press |
| Toggle Laser Beam (High / low) | `v_mining_use_permanent_modifier` | lalt+4 | — | — | — | press |
| Jettison Cargo | `v_jettison_volatile_cargo` | lalt+j | — | — | — | press |

## Vehicles - Salvage · FLIGHT

`spaceship_salvage` · 31 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Tractor Beam Vehicle- Increase Distance | `tractor_beam_vehicle_increase_distance` | — | mwheel_up | — | — | press |
| Tractor Beam Vehicle - Decrease Distance | `tractor_beam_vehicle_decrease_distance` | — | mwheel_down | — | — | press |
| Toggle Fire Focused | `v_salvage_toggle_fire_focused` | — | mouse1 | — | button1 | smart_toggle |
| Toggle Fire Left | `v_salvage_toggle_fire_left` | ralt+a | — | — | — | smart_toggle |
| Toggle Fire Right | `v_salvage_toggle_fire_right` | ralt+d | — | — | — | smart_toggle |
| Toggle Fire Fracture | `v_salvage_toggle_fire_fracture` | ralt+w | — | — | — | smart_toggle |
| Toggle Fire Disintegrate | `v_salvage_toggle_fire_disintegrate` | ralt+s | — | — | — | smart_toggle |
| Salvage Mode Gimbal (Toggle) | `v_salvage_toggle_gimbal_mode` | g | — | b | — | press |
| Salvage Mode Gimbal Reset | `v_salvage_reset_gimbal` | lalt+g | — | — | — | — |
| Increase Beam Spacing | `v_salvage_increase_beam_spacing` | — | — | — | — | press |
| Decrease Beam Spacing | `v_salvage_decrease_beam_spacing` | — | — | — | — | press |
| Relative Beam Spacing | `v_salvage_beam_spacing_rel` | maxis_z | — | — | — | — |
| Absolute Beam Spacing | `v_salvage_beam_spacing_abs` | — | — | — | — | — |
| Salvage Beam Axis (Toggle) | `v_salvage_toggle_beam_spacing_axis` | lalt+mouse2 | — | — | — | — |
| Cycle Focused Salvage Modifiers | `v_salvage_cycle_modifiers_focused` | — | mouse2 | — | — | — |
| Cycle Left Salvage Modifiers | `v_salvage_cycle_modifiers_left` | — | — | — | — | — |
| Cycle Right Salvage Modifiers | `v_salvage_cycle_modifiers_right` | — | — | — | — | — |
| Cycle Structural Salvage Modes | `v_salvage_cycle_modifiers_structural` | — | — | — | — | — |
| Focus all salvage heads | `v_salvage_focus_all_heads` | lalt+s | — | — | — | — |
| Focus left salvage head | `v_salvage_focus_left` | lalt+a | — | — | — | — |
| Focus right salvage head | `v_salvage_focus_right` | lalt+d | — | — | — | — |
| Focus Fracture tool | `v_salvage_focus_fracture` | lalt+w | — | — | — | — |
| Focus Disintegration tool | `v_salvage_focus_disintegrate` | — | — | — | — | — |
| Nudge left salvage tool up | `v_salvage_nudge_up__left` | — | — | — | — | — |
| Nudge left salvage tool down | `v_salvage_nudge_down__left` | — | — | — | — | — |
| Nudge left salvage tool left | `v_salvage_nudge_left__left` | — | — | — | — | — |
| Nudge left salvage tool right | `v_salvage_nudge_right__left` | — | — | — | — | — |
| Nudge right salvage tool up | `v_salvage_nudge_up__right` | — | — | — | — | — |
| Nudge right salvage tool down | `v_salvage_nudge_down__right` | — | — | — | — | — |
| Nudge right salvage tool left | `v_salvage_nudge_left__right` | — | — | — | — | — |
| Nudge right salvage tool right | `v_salvage_nudge_right__right` | — | — | — | — | — |

## Turret Movement

`turret_movement` · 17 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Pitch up | `turret_pitch_up` | — | — | — | — | — |
| Pitch down | `turret_pitch_down` | — | — | — | — | — |
| Pitch | `turret_pitch` | — | — | thumbry | y | — |
| Pitch | `turret_pitch_mouse` | — | maxis_y | — | — | — |
| Yaw left | `turret_yaw_left` | — | — | — | — | — |
| Yaw right | `turret_yaw_right` | — | — | — | — | — |
| Yaw | `turret_yaw` | — | — | thumbrx | x | — |
| Yaw | `turret_yaw_mouse` | — | maxis_x | — | — | — |
| Toggle Turret Mouse Movement (VJoy, FPS style) | `turret_toggle_mouse_mode` | q | — | — | — | smart_toggle |
| Turret Mouse Mode - Cycle Modes | `turret_mouse_mode_cycle` | q | — | — | — | press |
| Turret Mouse Mode - VJoy Dragging | `turret_mouse_mode_set_vjoy` | — | — | — | — | press |
| Turret Mouse Mode - Relative Dragging | `turret_mouse_mode_set_1to1` | — | — | — | — | press |
| Turret Mouse Mode - Pointer | `turret_mouse_mode_set_pointer` | — | — | — | — | press |
| Exit Remote Turret | `turret_remote_exit` | y | — | shoulderl+b | — | — |
| Turret Gyro Stabilization (Toggle) | `turret_gyromode` | e | — | — | — | press |
| Next Remote Turret | `turret_remote_cycle_next` | d | — | — | — | press |
| Previous Remote Turret | `turret_remote_cycle_prev` | a | — | — | — | press |

## Turret Advanced

`turret_advanced` · 9 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Turret E.S.P. Toggle On / Off | `turret_esp_toggle` | — | — | — | — | press |
| Turret E.S.P. - Enable Temporarily (Hold) | `turret_esp_hold` | — | — | — | — | hold |
| Recenter Turret (Hold) | `turret_recenter` | c | — | — | — | hold_no_retrigger |
| Turret - Speed Limiter - On/Off (Hold/Toggle) | `turret_limiter_toggle` | — | — | — | — | smart_toggle |
| Turret - Speed Limiter (rel) | `turret_limiter_rel` | — | lalt+maxis_z | — | — | — |
| Turret - Speed Limiter - Increase (rel) | `turret_limiter_rel_increase` | — | — | — | — | all |
| Turret - Speed Limiter - Decrease (rel) | `turret_limiter_rel_decrease` | — | — | — | — | all |
| Turret - Speed Limiter (abs) | `turret_limiter_abs` | — | — | — | — | — |
| Change Turret Position | `turret_change_position` | s | — | — | — | press |

## Vehicles - Weapons · FLIGHT

`spaceship_weapons` · 49 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Gimbal State - Toggle Locked / Unlocked | `v_weapon_gimbals_state_toggle` | g | — | — | — | tap |
| Gimbal State - Set Locked | `v_weapon_gimbals_state_set_locked` | — | — | — | — | press |
| Gimbal State - Set Unlocked | `v_weapon_gimbals_state_set_unlocked` | — | — | — | — | press |
| Gimbal State - Unlocked - Cycle Source (VJoy / View) | `v_weapon_gimbals_unlocked_cycle_source` | — | — | — | — | press |
| Aim Mode - Cycle | `v_weapon_aim_type_cycle` | ralt+g | — | — | — | tap |
| Aim Mode - Set to PIP Aiming | `v_weapon_aim_type_set_pip_aiming` | — | — | — | — | press |
| Aim Mode - Set to Painting | `v_weapon_aim_type_set_painting` | — | — | — | — | press |
| Aim Mode - Set to Automatic | `v_weapon_aim_type_set_auto` | — | — | — | — | press |
| Staggered Fire - Toggle On / Off | `v_weapon_staggered_fire_toggle` | — | — | — | — | tap |
| Staggered Fire - On | `v_weapon_staggered_fire_on` | — | — | — | — | press |
| Staggered Fire - Off | `v_weapon_staggered_fire_off` | — | — | — | — | press |
| Suppress Aim Assists (Hold) | `v_weapon_suppress_aim_assists_hold` | — | — | — | — | hold |
| Toggle Lead / Lag PIPs | `v_weapon_pip_toggle_lead_lag` | — | — | — | — | tap |
| Set Lag PIPs | `v_weapon_pip_set_lag` | — | — | — | — | tap |
| Set Lead PIPs | `v_weapon_pip_set_lead` | — | — | — | — | tap |
| PIP Combination Type: Toggle | `v_weapon_pip_combination_type_toggle` | — | — | — | — | tap |
| PIP Combination Type: Set One PIP Per Weapon | `v_weapon_pip_combination_type_set_single` | — | — | — | — | tap |
| PIP Combination Type: Set One PIP Per Weapon Type | `v_weapon_pip_combination_type_set_combined_weapon_group` | — | — | — | — | tap |
| PIP Precision Lines Toggle | `v_weapon_pip_prec_line_toggle` | — | — | — | — | tap |
| PIP Precision Lines On | `v_weapon_pip_prec_line_on` | — | — | — | — | tap |
| PIP Precision Lines Off | `v_weapon_pip_prec_line_off` | — | — | — | — | tap |
| PIP Fading Toggle | `v_weapon_pip_fade_toggle` | — | — | — | — | tap |
| PIP Fading On | `v_weapon_pip_fade_on` | — | — | — | — | tap |
| PIP Fading Off | `v_weapon_pip_fade_off` | — | — | — | — | tap |
| Gunnery UI Magnification Toggle | `v_weapon_ui_scale_toggle` | — | — | — | — | tap |
| Gunnery UI Magnification On | `v_weapon_ui_scale_on` | — | — | — | — | tap |
| Gunnery UI Magnification Off | `v_weapon_ui_scale_off` | — | — | — | — | tap |
| Manual Convergence Distance (rel.) | `v_weapon_convergence_distance_rel` | — | — | — | — | — |
| Manual Convergence Distance - Increase | `v_weapon_convergence_distance_rel_increase` | — | — | — | — | press |
| Manual Convergence Distance - Decrease | `v_weapon_convergence_distance_rel_decrease` | — | — | — | — | press |
| Manual Convergence Distance (abs.) | `v_weapon_convergence_distance_abs` | — | — | — | — | — |
| Manual Convergence Distance - Reset | `v_weapon_convergence_distance_set_default` | — | — | — | — | press |
| Weapon Preset - Fire | `v_weapon_preset_attack` | mouse1 | — | triggerr_btn | button1 | all |
| Weapon Presets - Fire Guns Group 1 | `v_weapon_preset_fire_guns0` | — | — | — | — | all |
| Weapon Presets - Fire Guns Group 2 | `v_weapon_preset_fire_guns1` | — | — | — | — | all |
| Weapon Presets - Fire Guns Group 3 | `v_weapon_preset_fire_guns2` | — | — | — | — | all |
| Weapon Presets - Fire Guns Group 4 | `v_weapon_preset_fire_guns3` | — | — | — | — | all |
| Weapon Presets - Next | `v_weapon_preset_next` | mwheel_down | — | triggerr_btn | button1 | press |
| Weapon Presets - Previous | `v_weapon_preset_prev` | mwheel_up | — | triggerr_btn | button1 | press |
| Weapon Presets - Next (Overflow) | `v_weapon_preset_next_overflow` | — | — | — | — | press |
| Weapon Presets - Previous (Overflow) | `v_weapon_preset_prev_overflow` | — | — | — | — | press |
| Weapon Presets - Set Guns Group 1 | `v_weapon_preset_guns0` | — | — | — | — | press |
| Weapon Presets - Set Guns Group 2 | `v_weapon_preset_guns1` | — | — | — | — | press |
| Weapon Presets - Set Guns Group 3 | `v_weapon_preset_guns2` | — | — | — | — | press |
| Weapon Presets - Set Guns Group 4 | `v_weapon_preset_guns3` | — | — | — | — | press |
| Weapon Presets - Set EMPs | `v_weapon_preset_emp` | — | — | — | — | press |
| Weapon Presets - Set Quantum Jammers (short range) | `v_weapon_preset_qid_jammer` | — | — | — | — | press |
| Weapon Presets - Set Quantum Snares / Pulse (long range) | `v_weapon_preset_qid_pulse` | — | — | — | — | press |
| Weapon Presets - Set QIDs | `v_weapon_preset_qid` | — | — | — | — | press |

## Vehicles - Missiles · FLIGHT

`spaceship_missiles` · 14 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Launch Missiles (Tap) | `v_weapon_toggle_launch_missile` | mouse1 | — | triggerr_btn | button1 | press |
| Launch Missiles (Hold) | `v_weapon_launch_missile` | — | — | — | — | delayed_press |
| Cycle Next Missile Type | `v_weapon_cycle_missile_fwd` | mwheel_down | — | triggerl_btn | button2 | press |
| Cycle Previous Missile Type | `v_weapon_cycle_missile_back` | mwheel_up | — | — | — | press |
| Increase Number of Armed Missiles | `v_weapon_increase_max_missiles` | g | — | — | — | tap |
| Decrease Number of Armed Missiles | `v_weapon_decrease_max_missiles` | — | — | — | — | tap |
| Reset Number of Armed Missiles | `v_weapon_reset_max_missiles` | lalt+g | — | — | — | tap |
| Bombs - Toggle Desired Impact Point (Tap) | `v_weapon_bombing_toggle_desired_impact_point` | lalt+b | — | — | — | tap |
| Bombs - Toggle Desired Impact Point (Hold) | `v_weapon_bombing_toggle_desired_impact_point_hold` | — | — | — | — | delayed_press |
| Bombs - Increase HUD Range | `v_weapon_bombing_hud_range_increase` | — | — | — | — | tap |
| Bombs - Decrease HUD Range | `v_weapon_bombing_hud_range_decrease` | — | — | — | — | tap |
| Bombs - Reset HUD Range | `v_weapon_bombing_hud_range_reset` | — | — | — | — | tap |
| Enable Cinematic Camera (Toggle) | `v_weapon_launch_missile_cinematic` | — | — | — | — | tap |
| Enable Cinematic Camera (Hold) | `v_weapon_launch_missile_cinematic_hold` | — | — | — | — | — |

## Vehicles - Shields and Countermeasures · FLIGHT

`spaceship_defensive` · 12 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Decoy - Launch Burst (tap), Set and Launch Burst (hold) | `v_weapon_countermeasure_decoy_launch` | h | — | x | button3 | — |
| Decoy - Increase Burst Size (tap) | `v_weapon_countermeasure_decoy_burst_increase` | ralt+h | — | — | — | press |
| Decoy - Decrease Burst Size (tap) | `v_weapon_countermeasure_decoy_burst_decrease` | lalt+h | — | — | — | press |
| Decoy - Panic Launch (tap) | `v_weapon_countermeasure_decoy_launch_panic` | — | — | — | — | press |
| Noise - Deploy (Tap) | `v_weapon_countermeasure_noise_launch` | j | — | — | button5 | press |
| Shield raise level front | `v_shield_raise_level_forward` | — | — | — | — | press |
| Shield raise level back | `v_shield_raise_level_back` | — | — | — | — | press |
| Shield raise level left | `v_shield_raise_level_left` | — | — | — | — | press |
| Shield raise level right | `v_shield_raise_level_right` | — | — | — | — | press |
| Shield raise level top | `v_shield_raise_level_up` | — | — | — | — | press |
| Shield raise level bottom | `v_shield_raise_level_down` | — | — | — | — | press |
| Shield reset levels | `v_shield_reset_level` | — | — | — | — | press |

## spaceship_auto_weapons

`spaceship_auto_weapons` · 1 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| v_weapon_toggle_ai | `v_weapon_toggle_ai` | slash | — | — | — | — |

## Flight - Power · FLIGHT

`spaceship_power` · 29 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Toggle Power - All | `v_power_toggle` | u | — | — | — | press |
| Set Power On | `v_power_set_on` | — | — | — | — | press |
| Set Power Off | `v_power_set_off` | — | — | — | — | press |
| Toggle Power - Thrusters | `v_power_toggle_thrusters` | i | — | — | — | press |
| Set Thrusters Power On | `v_power_set_thrusters_on` | — | — | — | — | press |
| Set Thrusters Power Off | `v_power_set_thrusters_off` | — | — | — | — | press |
| Toggle Power - Shields | `v_power_toggle_shields` | o | — | — | — | press |
| Set Shields Power On | `v_power_set_shields_on` | — | — | — | — | press |
| Set Shields Power Off | `v_power_set_shields_off` | — | — | — | — | press |
| Toggle Power - Weapons | `v_power_toggle_weapons` | p | — | — | — | press |
| Set Weapons Power On | `v_power_set_weapons_on` | — | — | — | — | press |
| Set Weapons Power Off | `v_power_set_weapons_off` | — | — | — | — | press |
| Decrease Throttle | `v_power_throttle_down` | f9 | — | — | — | — |
| Decrease Throttle to Min | `v_power_throttle_min` | f9 | — | — | — | double_tap |
| Increase Throttle | `v_power_throttle_up` | f10 | — | — | — | — |
| Increase Throttle to Max | `v_power_throttle_max` | f10 | — | — | — | double_tap |
| Engines - Increase (Tap) | `v_engineering_assignment_engine_increase` | f6 | — | — | — | tap |
| Engines - Decrease (Tap) | `v_engineering_assignment_engine_decrease` | f6+lalt | — | — | — | tap |
| Engines - Set to Max (Hold) | `v_engineering_assignment_engine_max` | f6 | — | — | — | — |
| Engines - Set to Min (Hold) | `v_engineering_assignment_engine_min` | f6+lalt | — | — | — | — |
| Shields - Increase (Tap) | `v_engineering_assignment_shields_increase` | f7 | — | — | — | tap |
| Shields - Decrease (Tap) | `v_engineering_assignment_shields_decrease` | f7+lalt | — | — | — | tap |
| Shields - Set to Max (Hold) | `v_engineering_assignment_shields_max` | f7 | — | — | — | — |
| Shields - Set to Min (Hold) | `v_engineering_assignment_shields_min` | f7+lalt | — | — | — | — |
| Weapons - Increase (Tap) | `v_engineering_assignment_weapons_increase` | f5 | — | — | — | tap |
| Weapons - Decrease (Tap) | `v_engineering_assignment_weapons_decrease` | f5+lalt | — | — | — | tap |
| Weapons - Set to Max (Hold) | `v_engineering_assignment_weapons_max` | f5 | — | — | — | — |
| Weapons - Set to Min (Hold) | `v_engineering_assignment_weapons_min` | f5+lalt | — | — | — | — |
| Reset Assignments | `v_engineering_assignment_reset` | f8 | — | — | — | press |

## Flight - HUD · FLIGHT

`spaceship_hud` · 27 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Cycle Pitch Ladder Mode | `v_cycle_pitch_ladder_mode` | — | — | — | — | press |
| mobiGlas (Toggle) | `mobiglas` | f1 | — | — | — | press |
| toggle_ar_mode | `toggle_ar_mode` | — | — | — | — | press |
| Scoreboard | `v_hud_open_scoreboard` | f1 | — | shoulderl+start | — | all |
| v_hud_interact_toggle | `v_hud_interact_toggle` | — | — | — | — | press |
| v_hud_cycle_mode_fwd | `v_hud_cycle_mode_fwd` | — | — | — | — | — |
| v_hud_cycle_mode_back | `v_hud_cycle_mode_back` | — | — | — | — | — |
| v_hud_focused_cycle_mode_fwd | `v_hud_focused_cycle_mode_fwd` | — | — | — | — | — |
| v_hud_focused_cycle_mode_back | `v_hud_focused_cycle_mode_back` | — | — | — | — | — |
| v_hud_left_panel_up | `v_hud_left_panel_up` | — | — | — | — | — |
| v_hud_left_panel_down | `v_hud_left_panel_down` | — | — | — | — | — |
| v_hud_left_panel_left | `v_hud_left_panel_left` | — | — | — | — | — |
| v_hud_left_panel_right | `v_hud_left_panel_right` | — | — | — | — | — |
| v_hud_confirm | `v_hud_confirm` | — | — | a | — | — |
| v_hud_cancel | `v_hud_cancel` | — | — | b | — | — |
| v_hud_stick_x | `v_hud_stick_x` | — | — | — | — | — |
| v_hud_stick_y | `v_hud_stick_y` | — | — | — | — | — |
| v_comm_open_chat | `v_comm_open_chat` | — | — | — | — | — |
| v_comm_show_chat | `v_comm_show_chat` | — | — | — | — | — |
| v_comm_open_precanned | `v_comm_open_precanned` | — | — | — | — | — |
| v_comm_select_precanned_1 | `v_comm_select_precanned_1` | — | — | — | — | — |
| v_comm_select_precanned_2 | `v_comm_select_precanned_2` | — | — | — | — | — |
| v_comm_select_precanned_3 | `v_comm_select_precanned_3` | — | — | — | — | — |
| v_comm_select_precanned_4 | `v_comm_select_precanned_4` | — | — | — | — | — |
| v_comm_select_precanned_5 | `v_comm_select_precanned_5` | — | — | — | — | — |
| Map | `v_starmap` | f2 | — | — | — | press |
| Wipe Helmet Visor | `visor_wipe` | lalt+x | — | — | — | press |

## Lights

`lights_controller` · 5 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Headlights (Toggle) | `v_lights` | l | — | — | — | press |
| Headlights\n(Enable) | `v_lights_on` | — | — | — | — | press |
| Headlights\n(Enable) | `v_lights_off` | — | — | — | — | press |
| v_toggle_running_lights | `v_toggle_running_lights` | — | — | — | — | smart_toggle |
| v_toggle_cabin_lights | `v_toggle_cabin_lights` | — | — | — | — | smart_toggle |

## Vehicle - Mobiglas · FLIGHT

`vehicle_mobiglas` · 28 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| ui_3d_display_select | `ui_3d_display_select` | — | mouse1 | — | — | tap |
| ui_3d_display_reorient | `ui_3d_display_reorient` | r | — | thumbr | — | tap |
| ui_3d_display_center | `ui_3d_display_center` | e | mouse1 | thumbl | — | double_tap_nonblocking |
| ui_3d_display_decenter | `ui_3d_display_decenter` | — | mouse2 | — | — | double_tap_nonblocking |
| ui_3d_display_zoom_out_button | `ui_3d_display_zoom_out_button` | np_subtract | — | triggerl_btn | — | — |
| ui_3d_display_zoom_in_button | `ui_3d_display_zoom_in_button` | np_add | — | triggerr_btn | — | — |
| ui_3d_display_zoom_in_analog | `ui_3d_display_zoom_in_analog` | — | — | — | — | — |
| ui_3d_display_zoom_out_analog | `ui_3d_display_zoom_out_analog` | — | — | — | — | — |
| ui_3d_display_zoom_out_wheel | `ui_3d_display_zoom_out_wheel` | — | mwheel_down | — | — | press |
| ui_3d_display_zoom_in_wheel | `ui_3d_display_zoom_in_wheel` | — | mwheel_up | — | — | press |
| ui_3d_display_pan_toggle | `ui_3d_display_pan_toggle` | — | mouse2 | — | — | — |
| ui_3d_display_rotate_toggle | `ui_3d_display_rotate_toggle` | — | mouse1 | — | — | — |
| ui_3d_display_zoom_toggle | `ui_3d_display_zoom_toggle` | — | mouse3 | triggerr_btn | — | — |
| ui_3d_display_toggledPanX | `ui_3d_display_toggledPanX` | — | maxis_x | — | — | — |
| ui_3d_display_toggledPanY | `ui_3d_display_toggledPanY` | — | maxis_y | — | — | — |
| ui_3d_display_toggledYaw | `ui_3d_display_toggledYaw` | — | maxis_x | — | — | — |
| ui_3d_display_toggledPitch | `ui_3d_display_toggledPitch` | — | maxis_y | — | — | — |
| ui_3d_display_toggledZoom | `ui_3d_display_toggledZoom` | — | maxis_y | — | — | — |
| ui_3d_display_nonToggledPanUp | `ui_3d_display_nonToggledPanUp` | w | — | thumbl_up | — | — |
| ui_3d_display_nonToggledPanDown | `ui_3d_display_nonToggledPanDown` | s | — | thumbl_down | — | — |
| ui_3d_display_nonToggledPanLeft | `ui_3d_display_nonToggledPanLeft` | a | — | thumbl_left | — | — |
| ui_3d_display_nonToggledPanRight | `ui_3d_display_nonToggledPanRight` | d | — | thumbl_right | — | — |
| ui_3d_display_nonToggledYawUp | `ui_3d_display_nonToggledYawUp` | up | — | thumbr_up | — | — |
| ui_3d_display_nonToggledYawDown | `ui_3d_display_nonToggledYawDown` | down | — | thumbr_down | — | — |
| ui_3d_display_nonToggledPitchLeft | `ui_3d_display_nonToggledPitchLeft` | left | — | thumbr_left | — | — |
| ui_3d_display_nonToggledPitchRight | `ui_3d_display_nonToggledPitchRight` | right | — | thumbr_right | — | — |
| ui_3d_display_pinMode | `ui_3d_display_pinMode` | lctrl | — | — | — | hold |
| ui_3d_display_pinSelect | `ui_3d_display_pinSelect` | — | mouse1 | — | — | tap |

## Stop Watch

`stopwatch` · 2 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Reset (Long Press) | `stopwatch_reset` | — | — | — | — | — |
| Start / Pause (Short Press) | `stopwatch_trigger` | — | — | — | — | — |

## On Foot - All · ON FOOT

`player` · 144 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Move Left | `moveleft` | a | — | — | — | hold |
| Move Right | `moveright` | d | — | — | — | hold |
| Move Forward | `moveforward` | w | — | — | — | hold |
| Move Backwards | `moveback` | s | — | — | — | hold |
| rotateyaw | `rotateyaw` | — | maxis_x | — | — | — |
| rotatepitch | `rotatepitch` | — | maxis_y | — | — | — |
| Move Left / Right | `gp_movex` | — | — | thumblx | — | — |
| Move Forward / Backward | `gp_movey` | — | — | thumbly | — | — |
| Look (Yaw) | `gp_rotateyaw` | — | — | thumbrx | — | — |
| Look (Pitch) | `gp_rotatepitch` | — | — | thumbry | — | — |
| Jump | `jump` | space | — | — | — | press |
| Jump Thrusters - Activate (hold) | `jump_hold` | space | — | — | — | — |
| Jump Thrusters - Release | `jump_release` | space | — | — | — | — |
| Crouch | `crouch` | c | — | — | — | hold_toggle |
| Jump | `gp_jump` | — | — | a | — | press |
| Crouch | `gp_crouch` | — | — | b | — | press |
| Prone | `prone` | lctrl | — | — | — | press |
| Sprint | `sprint` | lshift | — | thumbl | — | hold_toggle |
| Walk | `walk` | — | — | — | — | tap |
| Lean Left | `leanleft` | q | — | shoulderl+thumbl | — | hold |
| Lean Right | `leanright` | e | — | shoulderl+thumbr | — | hold |
| Climb Ledges | `ledgegrab` | space | — | a | — | hold |
| Firearm - Attack | `attack1` | — | mouse1 | triggerr_btn | — | all |
| Tool - Secondary Fire | `attackSecondary` | — | mouse2 | triggerr_btn | — | all |
| Melee - Attack Light Left | `melee_AttackLightLeft` | — | mouse1 | triggerl_btn | — | tap |
| Melee - Attack Light Right | `melee_AttackLightRight` | — | mouse2 | triggerr_btn | — | tap |
| Melee - Attack Heavy Left (Hold) | `melee_AttackHeavyLeft` | — | mouse1 | triggerl_btn | — | delayed_press |
| Melee - Attack Heavy Right (Hold) | `melee_AttackHeavyRight` | — | mouse2 | triggerr_btn | — | delayed_press |
| Melee - Block (Hold) | `melee_block` | — | mouse1_2 | triggerl_r_btn | — | hold |
| Medical Pen - Inject Other | `melee_AttackSyringeStab` | — | mouse2 | triggerr_btn | — | tap |
| Dodge left | `melee_dodgeLeft` | a | — | — | — | double_tap_nonblocking |
| Dodge Right | `melee_dodgeRight` | d | — | — | — | double_tap_nonblocking |
| Dodge Back | `melee_dodgeBack` | s | — | — | — | double_tap_nonblocking |
| restrain | `restrain` | — | — | — | — | tap |
| Melee - Attack (Ranged Weapon + Takedowns) | `weapon_melee` | — | mouse3 | thumbr | — | delayed_press |
| Melee - Attack (Ranged Weapon + Takedowns) | `takedown_nonLethal` | — | mouse3 | thumbr | — | delayed_press |
| takedown_lethal | `takedown_lethal` | — | mouse3 | thumbr | — | tap |
| Throw - Overarm & Two-Handed | `throw_overhand` | — | mouse1 | a | — | hold |
| Throw - Underarm | `throw_underhand` | — | mouse2 | b | — | hold |
| Aim Down Sight | `zoom` | — | mouse2 | triggerl_btn | — | all |
| Interact With Scope (ADS) | `interact_with_scope` | lshift | — | dpad_up | — | tap |
| Weapon Stance (Toggle) | `toggle_lowered` | lalt+r | — | — | — | tap |
| Select Primary Weapon | `select_primary_pit` | 1 | — | — | — | tap |
| Select Secondary Weapon | `select_secondary_pit` | 2 | — | dpad_left | — | tap |
| Select Sidearm | `select_sidearm_pit` | 3 | — | dpad_right | — | tap |
| Select Melee | `select_meleeweapon_pit` | v | — | shoulderl+dpad_right | — | tap |
| Select Gadget | `select_gadget_pit` | 5 | — | dpad_down | — | tap |
| Unarmed Combat | `selectUnarmedCombat` | 6 | — | shoulderl+dpad_left | — | tap |
| nextitem | `nextitem` | — | — | — | — | press |
| prevItem | `prevItem` | — | — | — | — | press |
| Next Weapon | `nextweapon` | — | — | — | — | press |
| Previous Weapon | `prevweapon` | — | — | — | — | press |
| Reload | `reload` | r | — | x | — | tap |
| Reload Secondary Fire | `reloadSecondary` | lalt+b | — | — | — | tap |
| Repool Ammunition | `ammoRepool` | lalt+1 | — | — | — | press |
| Holster Weapon | `holster` | r | — | x | — | delayed_press_medium |
| Drop Item | `drop` | — | — | — | — | — |
| Inspect Item | `inspect` | — | — | — | — | tap |
| Customize Weapon | `customize` | j | — | shoulderl+x | — | tap |
| Hold Breath (ADS) | `stabilize` | lshift | — | thumbl | — | hold |
| FPS Underbarrel Attachment Action | `weapon_auxiliary_action` | u | — | shoulderl+dpad_left | — | press |
| Change Fire Mode | `weapon_change_firemode` | b | — | shoulderl+dpad_up | — | press |
| Weapon Zeroing Decrease | `weapon_zeroing_decrease` | pgdn | — | — | — | press |
| Weapon Zeroing Increase / Auto | `weapon_zeroing_increase` | pgup | — | — | — | press |
| Default Movement Speed Increase | `fixed_speed_increment` | — | mwheel_up | — | — | press |
| Default Movement Speed Decrease | `fixed_speed_decrement` | — | mwheel_down | — | — | press |
| use | `use` | e | — | a | — | — |
| useAttachmentBottom | `useAttachmentBottom` | — | — | — | — | all |
| useAttachmentTop | `useAttachmentTop` | — | — | — | — | all |
| Request Rescue (while Incapacitated) | `downedRevivalRequest` | m | — | — | — | delayed_hold_long |
| Flashlight (Toggle) | `toggle_flashlight` | t | — | shoulderl+dpad_down | — | press |
| combathealtarget | `combathealtarget` | — | — | — | — | delayed_hold |
| Toggle Equip Helmet | `toggleEquipHelmet` | ralt+h | — | — | — | tap |
| Helmet\n(Equip) | `toggleAttachHelmet` | lalt+h | — | — | — | tap |
| Default Movement Speed Increase | `toggleHelmetState` | — | — | — | — | press |
| visor_next_mode | `visor_next_mode` | — | — | — | — | — |
| visor_prev_mode | `visor_prev_mode` | — | — | — | — | — |
| Wipe Helmet Visor | `visor_wipe` | lalt+x | — | — | — | press |
| selectitem | `selectitem` | — | mouse1 | a | — | press |
| cancelselect | `cancelselect` | — | mouse2 | b | — | press |
| Third Person View (Toggle) | `thirdperson` | f4 | — | shoulderl+y | — | tap |
| toggle_cursor_input | `toggle_cursor_input` | f11 | — | — | — | press |
| Free View Camera (Hold) | `free_thirdperson_camera` | z | — | — | — | hold |
| pan_thirdperson_up | `pan_thirdperson_up` | up | — | — | — | press |
| pan_thirdperson_down | `pan_thirdperson_down` | down | — | — | — | press |
| Zoom Out | `zoom_out` | — | mwheel_down | — | — | press |
| Zoom In | `zoom_in` | — | mwheel_up | — | — | press |
| break_conversation_effects | `break_conversation_effects` | — | — | — | — | — |
| hmd_rotateyaw | `hmd_rotateyaw` | HMD_Yaw | — | — | — | — |
| hmd_rotatepitch | `hmd_rotatepitch` | HMD_Pitch | — | — | — | — |
| hmd_rotateroll | `hmd_rotateroll` | HMD_Roll | — | — | — | — |
| mobiGlas (Toggle) | `mobiglas` | f1 | — | — | — | press |
| Recall Last Vehicle | `ship_recall` | — | — | — | — | press |
| Scoreboard | `pl_hud_open_scoreboard` | f1 | — | shoulderl+start | — | — |
| pl_hud_confirm | `pl_hud_confirm` | enter | — | a | — | press |
| toggle_ar_mode | `toggle_ar_mode` | — | — | — | — | press |
| ar_mode_scroll_action_up | `ar_mode_scroll_action_up` | — | mwheel_up | dpad_up | — | press |
| ar_mode_scroll_action_down | `ar_mode_scroll_action_down` | — | mwheel_down | dpad_down | — | press |
| shop_camera_zoom_in | `shop_camera_zoom_in` | — | mwheel_up | triggerl_btn | — | — |
| shop_camera_zoom_out | `shop_camera_zoom_out` | — | mwheel_down | triggerr_btn | — | — |
| shop_camera_mouseyaw | `shop_camera_mouseyaw` | — | maxis_x | — | — | — |
| shop_camera_mousepitch | `shop_camera_mousepitch` | — | maxis_y | — | — | — |
| spectate_enterpuremode | `spectate_enterpuremode` | — | — | — | — | delayed_press |
| Dismiss Corpse Marker | `dismiss_corpse_marker` | — | — | — | — | press |
| Firearm - Attack | `consume` | — | mouse1 | triggerr_btn | — | hold |
| ui_3d_display_select | `ui_3d_display_select` | — | mouse1 | — | — | tap |
| ui_3d_display_reorient | `ui_3d_display_reorient` | r | — | thumbr | — | tap |
| ui_3d_display_center | `ui_3d_display_center` | e | mouse1 | thumbl | — | double_tap_nonblocking |
| ui_3d_display_decenter | `ui_3d_display_decenter` | — | mouse2 | — | — | double_tap_nonblocking |
| ui_3d_display_zoom_out_button | `ui_3d_display_zoom_out_button` | np_subtract | — | triggerl_btn | — | — |
| ui_3d_display_zoom_in_button | `ui_3d_display_zoom_in_button` | np_add | — | triggerr_btn | — | — |
| ui_3d_display_zoom_in_analog | `ui_3d_display_zoom_in_analog` | — | — | — | — | — |
| ui_3d_display_zoom_out_analog | `ui_3d_display_zoom_out_analog` | — | — | — | — | — |
| ui_3d_display_zoom_out_wheel | `ui_3d_display_zoom_out_wheel` | — | mwheel_down | — | — | press |
| ui_3d_display_zoom_in_wheel | `ui_3d_display_zoom_in_wheel` | — | mwheel_up | — | — | press |
| ui_3d_display_pan_toggle | `ui_3d_display_pan_toggle` | — | mouse2 | — | — | — |
| ui_3d_display_rotate_toggle | `ui_3d_display_rotate_toggle` | — | mouse1 | — | — | — |
| ui_3d_display_zoom_toggle | `ui_3d_display_zoom_toggle` | — | mouse3 | triggerr_btn | — | — |
| ui_3d_display_toggledPanX | `ui_3d_display_toggledPanX` | — | maxis_x | — | — | — |
| ui_3d_display_toggledPanY | `ui_3d_display_toggledPanY` | — | maxis_y | — | — | — |
| ui_3d_display_toggledYaw | `ui_3d_display_toggledYaw` | — | maxis_x | — | — | — |
| ui_3d_display_toggledPitch | `ui_3d_display_toggledPitch` | — | maxis_y | — | — | — |
| ui_3d_display_toggledZoom | `ui_3d_display_toggledZoom` | — | maxis_y | — | — | — |
| ui_3d_display_nonToggledPanUp | `ui_3d_display_nonToggledPanUp` | w | — | thumbl_up | — | — |
| ui_3d_display_nonToggledPanDown | `ui_3d_display_nonToggledPanDown` | s | — | thumbl_down | — | — |
| ui_3d_display_nonToggledPanLeft | `ui_3d_display_nonToggledPanLeft` | a | — | thumbl_left | — | — |
| ui_3d_display_nonToggledPanRight | `ui_3d_display_nonToggledPanRight` | d | — | thumbl_right | — | — |
| ui_3d_display_nonToggledYawUp | `ui_3d_display_nonToggledYawUp` | up | — | thumbr_up | — | — |
| ui_3d_display_nonToggledYawDown | `ui_3d_display_nonToggledYawDown` | down | — | thumbr_down | — | — |
| ui_3d_display_nonToggledPitchLeft | `ui_3d_display_nonToggledPitchLeft` | left | — | thumbr_left | — | — |
| ui_3d_display_nonToggledPitchRight | `ui_3d_display_nonToggledPitchRight` | right | — | thumbr_right | — | — |
| ui_3d_display_pinMode | `ui_3d_display_pinMode` | lctrl | — | — | — | hold |
| ui_3d_display_pinSelect | `ui_3d_display_pinSelect` | — | mouse1 | — | — | tap |
| Port Modification Interact | `port_modification_select` | — | mouse1 | a | — | press |
| Map | `v_starmap` | f2 | — | — | — | press |
| Force Re-spawn (E.V.A. / On Foot) | `force_respawn` | backspace | — | — | — | delayed_press_medium |
| pc_conversation_option1 | `pc_conversation_option1` | 1 | — | — | — | all |
| pc_conversation_option2 | `pc_conversation_option2` | 2 | — | — | — | all |
| pc_conversation_option3 | `pc_conversation_option3` | 3 | — | — | — | all |
| pc_conversation_option4 | `pc_conversation_option4` | 4 | — | — | — | all |
| pc_conversation_option5 | `pc_conversation_option5` | 5 | — | — | — | all |
| pc_conversation_option_up | `pc_conversation_option_up` | — | mwheel_up | — | — | all |
| pc_conversation_option_down | `pc_conversation_option_down` | — | mwheel_down | — | — | all |
| pc_conversation_option_select | `pc_conversation_option_select` | — | mouse1 | — | — | all |

## On Foot - All · ON FOOT

`prone` · 2 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Roll Left (while Prone) | `prone_rollleft` | — | — | — | — | — |
| Roll Right (while Prone) | `prone_rollright` | — | — | — | — | — |

## mapui

`mapui` · 19 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| mapui_pan_left | `mapui_pan_left` | a | — | — | — | hold |
| mapui_pan_right | `mapui_pan_right` | d | — | — | — | hold |
| mapui_pan_forward | `mapui_pan_forward` | w | — | — | — | hold |
| mapui_pan_back | `mapui_pan_back` | s | — | — | — | hold |
| mapui_pan_up | `mapui_pan_up` | space | — | — | — | — |
| mapui_pan_down | `mapui_pan_down` | lctrl | — | — | — | — |
| mapui_cycle_section_forward | `mapui_cycle_section_forward` | e | — | — | — | — |
| mapui_cycle_section_backward | `mapui_cycle_section_backward` | q | — | — | — | — |
| mapui_cycle_zone_forward | `mapui_cycle_zone_forward` | lshift+e | — | — | — | — |
| mapui_cycle_zone_backward | `mapui_cycle_zone_backward` | lshift+q | — | — | — | — |
| mapui_action_planroute | `mapui_action_planroute` | r | — | — | — | — |
| mapui_action_clearroute | `mapui_action_clearroute` | c | — | — | — | — |
| mapui_action_togglepin | `mapui_action_togglepin` | t | — | — | — | — |
| mapui_action_mylocation | `mapui_action_mylocation` | 2 | — | — | — | — |
| mapui_action_toggle_view_entire_zone | `mapui_action_toggle_view_entire_zone` | z | — | — | — | — |
| mapui_action_toggleQTActions | `mapui_action_toggleQTActions` | lshift | — | — | — | hold |
| mapui_action_goto_selection | `mapui_action_goto_selection` | 3 | — | — | — | — |
| mapui_action_step_back | `mapui_action_step_back` | 4 | — | — | — | — |
| mapui_action_goto_localmap | `mapui_action_goto_localmap` | 1 | — | — | — | — |

## hacking

`hacking` · 22 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| hacking_minigame_debug_toggle_command_input | `hacking_minigame_debug_toggle_command_input` | rshift | — | — | — | double_tap |
| hacking_minigame_debug_mouse_x | `hacking_minigame_debug_mouse_x` | — | maxis_x | — | — | — |
| hacking_minigame_debug_mouse_y | `hacking_minigame_debug_mouse_y` | — | maxis_y | — | — | — |
| hacking_minigame_mouse_lmb | `hacking_minigame_mouse_lmb` | — | mouse1 | — | — | hold |
| hacking_minigame_mouse_rmb | `hacking_minigame_mouse_rmb` | — | mouse2 | — | — | press |
| hacking_minigame_abort | `hacking_minigame_abort` | y | — | — | — | press |
| hacking_minigame_help_window_toggle | `hacking_minigame_help_window_toggle` | h | — | — | — | press |
| hacking_minigame_camera_control | `hacking_minigame_camera_control` | — | mouse3 | — | — | hold |
| hacking_minigame_camera_x | `hacking_minigame_camera_x` | — | maxis_x | — | — | — |
| hacking_minigame_camera_y | `hacking_minigame_camera_y` | — | maxis_y | — | — | — |
| hacking_minigame_movement_up | `hacking_minigame_movement_up` | w | — | — | — | hold |
| hacking_minigame_movement_down | `hacking_minigame_movement_down` | s | — | — | — | hold |
| hacking_minigame_movement_left | `hacking_minigame_movement_left` | a | — | — | — | hold |
| hacking_minigame_movement_right | `hacking_minigame_movement_right` | d | — | — | — | hold |
| hacking_minigame_swap_rotate_cw | `hacking_minigame_swap_rotate_cw` | — | mwheel_down | — | — | press |
| hacking_minigame_swap_rotate_ccw | `hacking_minigame_swap_rotate_ccw` | — | mwheel_up | — | — | press |
| hacking_minigame_ability_inject | `hacking_minigame_ability_inject` | f | — | — | — | press |
| hacking_minigame_ability_ping | `hacking_minigame_ability_ping` | tab | — | — | — | delayed_press |
| hacking_minigame_ability_slowdown | `hacking_minigame_ability_slowdown` | q | — | — | — | press |
| hacking_minigame_ability_swap | `hacking_minigame_ability_swap` | r | — | — | — | press |
| hacking_minigame_ability_wraparound | `hacking_minigame_ability_wraparound` | e | — | — | — | press |
| hacking_minigame_cycle_input_mode | `hacking_minigame_cycle_input_mode` | i | — | — | — | press |

## On Foot - All · ON FOOT

`tractor_beam` · 10 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Tractor Beam - Increase Distance | `tractor_beam_increase_distance` | — | mwheel_up | — | button3 | press |
| Tractor Beam - Decrease Distance | `tractor_beam_decrease_distance` | — | mwheel_down | — | button4 | press |
| tractor_beam_rotate | `tractor_beam_rotate` | r | — | — | — | hold |
| tractor_beam_rotate_x | `tractor_beam_rotate_x` | — | maxis_x | — | — | — |
| tractor_beam_rotate_y | `tractor_beam_rotate_y` | — | maxis_y | — | — | — |
| tractor_beam_rotate_z_up | `tractor_beam_rotate_z_up` | — | mwheel_up | — | — | press |
| tractor_beam_rotate_z_down | `tractor_beam_rotate_z_down` | — | mwheel_down | — | — | press |
| tractor_beam_detach | `tractor_beam_detach` | b | — | — | — | press |
| tractor_beam_throw | `tractor_beam_throw` | mouse2 | — | — | — | all |
| tractor_beam_reset_rotation | `tractor_beam_reset_rotation` | mouse3 | — | — | — | press |

## mining

`mining` · 1 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| ui_CIFPSIncreaseMiningThrottle | `weapon_change_mining_throttle` | lalt+maxis_z | — | — | button3 | press |

## On Foot - All · ON FOOT

`incapacitated` · 1 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Regen (while Incapacitated) | `incapacitatedRespawn` | backspace | — | x | — | delayed_hold_long |

## E.V.A - All · E.V.A.

`zero_gravity_eva` · 23 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| View Left | `eva_view_yaw_left` | — | — | — | — | hold |
| View Right | `eva_view_yaw_right` | — | — | — | — | hold |
| View Left/Right | `eva_view_yaw` | — | — | thumbrx | — | — |
| View Left/Right | `eva_view_yaw_mouse` | — | maxis_x | — | — | — |
| View Up | `eva_view_pitch_up` | — | — | — | — | hold |
| View Down | `eva_view_pitch_down` | — | — | — | — | hold |
| View Up/Down | `eva_view_pitch` | — | — | thumbry | — | — |
| View Up/Down | `eva_view_pitch_mouse` | — | maxis_y | — | — | — |
| Roll Left | `eva_roll_left` | q | — | — | — | hold |
| Roll Right | `eva_roll_right` | e | — | — | — | hold |
| Roll Left/Right | `eva_roll` | — | — | shoulderl+thumblx | — | — |
| Strafe Up | `eva_strafe_up` | space | — | — | — | hold |
| Strafe Down | `eva_strafe_down` | lctrl | — | — | — | hold |
| Strafe Up/Down | `eva_strafe_vertical` | — | — | shoulderl+thumbry | — | — |
| Strafe Left | `eva_strafe_left` | a | — | — | — | hold |
| Strafe Right | `eva_strafe_right` | d | — | — | — | hold |
| Strafe Left/Right | `eva_strafe_lateral` | — | — | thumblx | — | — |
| Strafe Forward | `eva_strafe_forward` | w | — | — | — | hold |
| Strafe Backward | `eva_strafe_back` | s | — | — | — | hold |
| Strafe Forward/Backward | `eva_strafe_longitudinal` | — | — | thumbly | — | — |
| Brake | `eva_brake` | x | — | — | — | hold |
| Boost | `eva_boost` | lshift | — | thumbl | — | hold |
| Freelook (Hold) | `eva_toggle_headlook_mode` | z | — | — | — | hold |

## E.V.A. - Zero-G Traversal

`zero_gravity_traversal` · 4 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Launch from Surface | `zgt_launch` | space | — | — | — | hold |
| Detach from Surface | `zgt_detach` | y | — | shoulderl+b | — | press |
| Roll Left | `zgt_roll_left` | q | — | — | — | press |
| Roll Right | `zgt_roll_right` | e | — | — | — | press |

## Ground Vehicle - General · Vehicle

`vehicle_general` · 27 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Horn | `v_horn` | space | — | dpad_down | button8 | — |
| Cycle camera view | `v_view_cycle_fwd` | f4 | — | shoulderl+y | — | tap |
| v_view_option | `v_view_option` | — | — | — | — | — |
| Zoom in (3rd person view) | `v_view_zoom_in` | — | mwheel_up | — | — | — |
| Zoom out (3rd person view) | `v_view_zoom_out` | — | mwheel_down | — | — | — |
| Look left / right | `v_view_yaw_mouse` | — | maxis_x | — | — | — |
| Look up / down | `v_view_pitch_mouse` | — | maxis_y | — | — | — |
| Look left / right | `v_view_yaw` | — | — | thumbrx | x | — |
| Look up / down | `v_view_pitch` | — | — | thumbry | y | — |
| Freelook (Hold) | `v_view_freelook_mode` | z | — | — | — | hold |
| v_toggle_cursor_input | `v_toggle_cursor_input` | f11 | — | — | — | tap |
| v_view_yaw_absolute | `v_view_yaw_absolute` | HMD_Yaw | — | — | — | — |
| v_view_pitch_absolute | `v_view_pitch_absolute` | HMD_Pitch | — | — | — | — |
| v_view_roll_absolute | `v_view_roll_absolute` | HMD_Roll | — | — | — | — |
| mobiGlas (Toggle) | `mobiglas` | f1 | — | — | — | press |
| Flight / Systems Ready | `v_flightready` | ralt+r | — | shoulderl+shoulderr | — | press |
| Open/Close Doors (Toggle) | `v_toggle_all_doors` | — | — | — | — | press |
| Open All Doors | `v_open_all_doors` | — | — | — | — | press |
| Close All Doors | `v_close_all_doors` | — | — | — | — | press |
| Lock/Unlock Doors (Toggle) | `v_toggle_all_doorlocks` | — | — | — | — | press |
| Lock All Doors | `v_lock_all_doors` | — | — | — | — | press |
| Unlock All Doors | `v_unlock_all_doors` | — | — | — | — | press |
| Port Lock Toggle All | `v_toggle_all_portlocks` | ralt+K | — | — | — | press |
| Port Lock All | `v_lock_all_ports` | — | — | — | — | press |
| Port Unlock All | `v_unlock_all_ports` | — | — | — | — | press |
| Map | `v_starmap` | f2 | — | — | — | press |
| Wipe Helmet Visor | `visor_wipe` | lalt+x | — | — | — | press |

## Ground Vehicle - Movement · Vehicle

`vehicle_driver` · 20 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Drive Forward | `v_move_forward` | w | — | — | — | — |
| Drive Backward | `v_move_back` | s | — | — | — | — |
| Drive Forward / Backward | `v_move` | — | — | thumbly | slider1 | — |
| Turn Left | `v_yaw_left` | a | — | — | — | — |
| Turn Right | `v_yaw_right` | d | — | — | — | — |
| Yaw Left / Right (Axis / HOTAS) | `v_yaw` | — | — | thumblx | x | — |
| Yaw Left / Right (Mouse) | `v_yaw_mouse` | — | — | — | — | — |
| Ground Vehicles - Pitch Up | `v_pitch_up` | — | — | — | — | — |
| Ground Vehicles - Pitch Down | `v_pitch_down` | — | — | — | — | — |
| Pitch Up / Down (Axis / HOTAS) | `v_pitch` | — | — | thumblx | y | — |
| Pitch Up / Down (Mouse) | `v_pitch_mouse` | — | — | — | — | — |
| Brake | `v_brake` | x | — | shoulderl+thumbr | button7 | hold |
| Dynamic Zoom In and Out (rel.) | `v_view_dynamic_zoom_rel` | — | — | — | — | — |
| Dynamic Zoom In (rel.) | `v_view_dynamic_zoom_rel_in` | — | — | — | — | all |
| Dynamic Zoom Out (rel.) | `v_view_dynamic_zoom_rel_out` | — | — | — | — | all |
| Dynamic Zoom In and Out (abs.) | `v_view_dynamic_zoom_abs` | — | — | — | — | — |
| Dynamic Zoom Toggle (abs.) | `v_view_dynamic_zoom_abs_toggle` | — | — | — | — | hold |
| Boost | `v_boost` | lshift | — | thumbl | button8 | all |
| Lock Pitch / Yaw Movement (Toggle / Hold) | `v_lock_rotation` | rshift | — | — | — | smart_toggle |
| Toggle Auto Braking On Idle | `v_mgv_switch_brake_on_idle` | c | — | — | — | press |

## debug

`debug` · 14 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| godmode | `godmode` | f9 | — | — | — | — |
| debug_pause | `debug_pause` | pause | — | — | — | — |
| pause_and_fly | `pause_and_fly` | rctrl+p | — | — | — | — |
| debug_pause_alt | `debug_pause_alt` | rctrl+o | — | — | — | — |
| debug_time_slower | `debug_time_slower` | rctrl+minus | — | — | — | — |
| debug_time_faster | `debug_time_faster` | rctrl+equals | — | — | — | — |
| teleport_to_camera | `teleport_to_camera` | rctrl+t | — | — | — | — |
| toggleaidebugdraw | `toggleaidebugdraw` | f11 | — | — | — | — |
| ai_DebugCenterViewAgent | `ai_DebugCenterViewAgent` | np_divide | — | — | — | — |
| togglepdrawhelpers | `togglepdrawhelpers` | f10 | — | — | — | — |
| mannequin_debugai | `mannequin_debugai` | np_multiply | — | — | — | — |
| pl_result_state_debug_target | `pl_result_state_debug_target` | np_period | — | — | — | press |
| mov_advance_all_sequences | `mov_advance_all_sequences` | end | — | — | — | press |
| mov_pause_resume_all_sequences | `mov_pause_resume_all_sequences` | home | — | — | — | press |

## IFCS_controls

`IFCS_controls` · 4 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| v_IFCS_A | `v_IFCS_A` | rctrl+a | — | a | — | — |
| v_IFCS_B | `v_IFCS_B` | rctrl+b | — | b | — | — |
| v_IFCS_X | `v_IFCS_X` | rctrl+x | — | x | — | — |
| v_IFCS_Y | `v_IFCS_Y` | rctrl+y | — | y | — | — |

## Electronic Access - Spectator · Electronic Access - Spectator

`spectator` · 28 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Spectator Camera Target (Next) | `spectate_next_target` | — | mouse2 | dpad_right | button1 | press |
| Spectator Camera Target (Previous) | `spectate_prev_target` | — | mouse1 | dpad_left | button2 | press |
| Spectator Camera Lock Target | `spectate_toggle_lock_target` | 1 | — | a | — | press |
| Spectator Camera Zoom | `spectate_zoom` | — | maxis_z | — | — | — |
| Spectator Camera Zoom In | `spectate_zoom_in` | — | mwheel_up | dpad_up | hat1_up | — |
| Spectator Camera Zoom Out | `spectate_zoom_out` | — | mwheel_down | dpad_down | hat1_down | — |
| Spectator Camera Rotate Yaw | `spectate_rotateyaw_mouse` | — | maxis_x | — | x | — |
| Spectator Camera Rotate Pitch | `spectate_rotatepitch_mouse` | — | maxis_y | — | y | — |
| Spectator Camera Rotate Yaw | `spectate_rotateyaw` | — | — | thumbrx | x | — |
| Spectator Camera Rotate Pitch | `spectate_rotatepitch` | — | — | thumbry | y | — |
| Spectator Camera HUD (Toggle) | `spectate_toggle_hud` | b | — | b | — | press |
| spectate_gen_nextcamera | `spectate_gen_nextcamera` | n | — | — | — | press |
| Spectator Camera Mode (Next) | `spectate_gen_nextmode` | f4 | — | shoulderl+y | — | press |
| Spectator Camera Mode (Previous) | `spectate_gen_prevmode` | — | — | — | — | press |
| spectate_moveleft | `spectate_moveleft` | a | — | thumbl_left | — | hold |
| spectate_moveright | `spectate_moveright` | d | — | thumbl_right | — | hold |
| spectate_moveforward | `spectate_moveforward` | w | — | thumbl_up | — | hold |
| spectate_moveback | `spectate_moveback` | s | — | thumbl_down | — | hold |
| spectate_moveup | `spectate_moveup` | space | — | shoulderl+thumbr_up | — | hold |
| spectate_movedown | `spectate_movedown` | lctrl | — | shoulderl+thumbr_down | — | hold |
| spectate_freecam_sprint | `spectate_freecam_sprint` | lshift | — | thumbl | — | hold |
| spectate_toggle_freecam | `spectate_toggle_freecam` | z | — | thumbr | — | press |
| spectate_toggle_thirdperson | `spectate_toggle_thirdperson` | f4 | — | shoulderl+y | — | press |
| spectate_roll_left | `spectate_roll_left` | q | — | shoulderl+thumbl_left | — | hold |
| spectate_roll_right | `spectate_roll_right` | e | — | shoulderl+thumbl_right | — | hold |
| spectate_speed_increment | `spectate_speed_increment` | — | mwheel_up | — | — | press |
| spectate_speed_decrement | `spectate_speed_decrement` | — | mwheel_down | — | — | press |
| spectate_free_look | `spectate_free_look` | z | — | — | — | hold |

## Social - General · Social - General

`default` · 57 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| skip_cutscene | `skip_cutscene` | — | — | — | — | press |
| cam_toggle_cinematic | `cam_toggle_cinematic` | backspace | — | shoulderl+back | — | tap |
| objectives | `objectives` | o | — | — | — | — |
| toggle_trackview | `toggle_trackview` | ] | — | — | — | press |
| toggle_action_profile | `toggle_action_profile` | — | — | — | — | press |
| Re-spawn | `respawn` | f | — | x | button1 | all |
| retry | `retry` | x | — | x | button1 | press |
| ready | `ready` | x | — | x | button1 | press |
| Exit seat | `pl_exit` | y | — | shoulderl+b | — | — |
| flymode | `flymode` | f3 | — | — | — | — |
| flymode_strafe_up | `flymode_strafe_up` | space | — | — | — | — |
| flymode_strafe_down | `flymode_strafe_down` | lctrl | — | — | — | — |
| flymode_roll_left | `flymode_roll_left` | q | — | — | — | — |
| flymode_roll_right | `flymode_roll_right` | e | — | — | — | — |
| ui_toggle_pause | `ui_toggle_pause` | escape | — | start | — | — |
| ui_click | `ui_click` | enter | — | a | — | — |
| ui_back | `ui_back` | escape | — | b | — | — |
| ui_up | `ui_up` | up | — | — | — | — |
| ui_down | `ui_down` | down | — | — | — | — |
| ui_left | `ui_left` | left | — | — | — | — |
| ui_right | `ui_right` | right | — | — | — | — |
| ui_select | `ui_select` | enter | — | a | button1 | — |
| ui_secondary_select | `ui_secondary_select` | — | — | x | button2 | — |
| ui_radialmenu_pageleft | `ui_radialmenu_pageleft` | left | — | — | — | — |
| ui_radialmenu_pageright | `ui_radialmenu_pageright` | right | — | — | — | — |
| ui_confirm | `ui_confirm` | — | — | shoulderr | — | — |
| ui_reset | `ui_reset` | — | — | x | — | — |
| ui_skip_video | `ui_skip_video` | space | — | shoulderr | — | — |
| ui_hide_hint | `ui_hide_hint` | f | — | a | button2 | press |
| ui_primaryTab_increment | `ui_primaryTab_increment` | e | — | shoulderr | — | — |
| ui_primaryTab_decrement | `ui_primaryTab_decrement` | q | — | shoulderl | — | — |
| ui_secondaryTab_increment | `ui_secondaryTab_increment` | — | — | triggerr_btn | — | — |
| ui_secondaryTab_decrement | `ui_secondaryTab_decrement` | — | — | triggerl_btn | — | — |
| ui_focus_increment | `ui_focus_increment` | tab | — | x | — | — |
| ui_focus_decrement | `ui_focus_decrement` | lshift+tab | — | y | — | — |
| flashui_mouse | `flashui_mouse` | — | — | — | — | — |
| flashui_return | `flashui_return` | — | — | a | button2 | — |
| flashui_backspace | `flashui_backspace` | backspace | — | b | button3 | — |
| flashui_spacebar | `flashui_spacebar` | space | — | — | — | — |
| flashui_tab | `flashui_tab` | tab | — | thumbl | — | — |
| flashui_kp_2 | `flashui_kp_2` | — | — | x | button7 | — |
| flashui_kp_3 | `flashui_kp_3` | — | — | y | button4 | — |
| flashui_kp_4 | `flashui_kp_4` | — | — | dpad_left | button5 | — |
| flashui_kp_7 | `flashui_kp_7` | — | — | dpad_right | button6 | — |
| flashui_up | `flashui_up` | up | — | — | hat1_up | — |
| flashui_down | `flashui_down` | down | — | — | hat1_down | — |
| flashui_left | `flashui_left` | left | — | — | hat1_left | — |
| flashui_right | `flashui_right` | right | — | — | hat1_right | — |
| Notifications - Accept Prompt | `notification_accept` | lbracket | — | — | — | press |
| Notifications - Decline Prompt | `notification_decline` | rbracket | — | — | — | press |
| CommLink App (Toggle) | `toggle_contact` | f11 | — | — | — | press |
| Chat Window (Toggle) | `toggle_chat` | f12 | — | — | — | press |
| Cycle Chat Lobby | `cycle_chat_lobby` | tab | — | — | — | press |
| Chat Window Focus | `focus_on_chat_textinput` | — | — | — | — | press |
| ui_copy | `ui_copy` | lctrl+c | — | — | — | press |
| ui_cut | `ui_cut` | lctrl+x | — | — | — | press |
| ui_paste | `ui_paste` | lctrl+v | — | — | — | press |

## ui_textfield

`ui_textfield` · 6 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| ui_textfield_enter | `ui_textfield_enter` | — | — | — | — | — |
| ui_textfield_backspace | `ui_textfield_backspace` | backspace | — | — | — | — |
| ui_textfield_arrow_up | `ui_textfield_arrow_up` | up | — | — | — | — |
| ui_textfield_arrow_down | `ui_textfield_arrow_down` | down | — | — | — | — |
| ui_textfield_arrow_left | `ui_textfield_arrow_left` | left | — | — | — | — |
| ui_textfield_arrow_right | `ui_textfield_arrow_right` | right | — | — | — | — |

## Social - Invites · Social - General

`ui_notification` · 3 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Accept Invite | `ui_notification_accept` | lbracket | — | — | — | — |
| Reject Invite | `ui_notification_decline` | rbracket | — | — | — | tap |
| Ignore Invite (hold) | `ui_notification_ignore` | rbracket | — | — | — | delayed_hold |

## Social - Emotes · ON FOOT

`player_emotes` · 40 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Forward | `emote_cs_forward` | np_5 | — | — | — | press |
| Left | `emote_cs_left` | np_1 | — | — | — | press |
| Right | `emote_cs_right` | np_3 | — | — | — | press |
| Stop | `emote_cs_stop` | np_2 | — | — | — | press |
| Yes | `emote_cs_yes` | np_4 | — | — | — | press |
| No | `emote_cs_no` | np_6 | — | — | — | press |
| Agree | `emote_agree` | — | — | — | — | press |
| Angry | `emote_angry` | — | — | — | — | press |
| At Ease | `emote_atease` | — | — | — | — | press |
| Attention | `emote_attention` | — | — | — | — | press |
| Blah | `emote_blah` | — | — | — | — | press |
| Bored | `emote_bored` | — | — | — | — | press |
| Bow | `emote_bow` | — | — | — | — | press |
| Burp | `emote_burp` | — | — | — | — | press |
| Cheer | `emote_cheer` | — | — | — | — | press |
| Chicken | `emote_chicken` | — | — | — | — | press |
| Clap | `emote_clap` | — | — | — | — | press |
| Come | `emote_come` | — | — | — | — | press |
| Cry | `emote_cry` | — | — | — | — | press |
| Dance | `emote_dance` | — | — | — | — | press |
| Disagree | `emote_disagree` | — | — | — | — | press |
| Failure | `emote_failure` | — | — | — | — | press |
| Flex | `emote_flex` | — | — | — | — | press |
| Flirt | `emote_flirt` | — | — | — | — | press |
| Gasp | `emote_gasp` | — | — | — | — | press |
| Gloat | `emote_gloat` | — | — | — | — | press |
| Greet | `emote_greet` | — | — | — | — | press |
| Laugh | `emote_laugh` | — | — | — | — | press |
| Confirm Launch | `emote_launch` | — | — | — | — | press |
| Point | `emote_point` | — | — | — | — | press |
| Rude | `emote_rude` | — | — | — | — | press |
| Salute | `emote_salute` | — | — | — | — | press |
| Sit | `emote_sit` | — | — | — | — | press |
| Sleep | `emote_sleep` | — | — | — | — | press |
| Smell | `emote_smell` | — | — | — | — | press |
| Taunt | `emote_taunt` | — | — | — | — | press |
| Threaten | `emote_threaten` | — | — | — | — | press |
| Wait | `emote_wait` | — | — | — | — | press |
| Wave | `emote_wave` | — | — | — | — | press |
| Whistle | `emote_whistle` | — | — | — | — | press |

## VOIP, FOIP and Head Tracking · VOIP, FOIP and Head Tracking

`player_input_optical_tracking` · 13 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| [Experimental] VR - Toggle On / Off | `hmd_toggle` | np_divide | — | — | — | press |
| [Experimental] VR - Recenter Device | `hmd_recenter` | np_5 | — | — | — | press |
| [Experimental] VR - Toggle Theater Mode | `hmd_theater_mode_toggle` | lalt+np_5 | — | — | — | press |
| [Experimental] VR - Visor Toggle On / Off | `hmd_lens_display_toggle` | — | — | — | — | press |
| Enable Head Tracking (Toggle) | `headtrack_enabled` | np_divide | — | — | — | press |
| Head Tracking (Hold) | `headtrack_hold` | — | — | — | — | hold |
| Recenter Head Tracking Device (except TrackIR) | `headtrack_recenter_device` | np_5 | — | — | — | press |
| Enable / Disable Head Tracking for 3rd Person Camera (Toggle) | `headtrack_camera_enabled` | — | — | — | — | press |
| VOIP Push To Talk | `foip_pushtotalk` | np_add | — | — | — | hold |
| VOIP Push To Talk (Proximity only) | `foip_pushtotalk_proximity` | lalt+np_add | — | — | — | hold |
| FOIP Selfie Cam | `foip_viewownplayer` | np_subtract | — | — | — | delayed_hold |
| FOIP Recalibrate | `foip_recalibrate` | np_multiply | — | — | — | press |
| Cycle through audio channels | `foip_cyclechannel` | np_period | — | — | — | press |

## Quick Keys, Interactions, and Inner Thought · Quick Keys, Interactions, and Inner Thought

`player_choice` · 39 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| pc_item_primary | `pc_item_primary` | — | mouse1 | — | — | — |
| pc_item_secondary | `pc_item_secondary` | — | mouse2 | — | — | — |
| Interaction Mode | `pc_interaction_mode` | f | — | — | button6 | — |
| Activate Inner Thought | `pc_interaction_select` | — | mouse2 | — | — | — |
| Activate Inner Thought | `pc_select` | — | mouse1 | a | button1 | — |
| Focus | `pc_focus` | — | mouse3 | triggerl_btn | button7 | tap |
| Interaction Mode Zoom In | `pc_zoom_in` | — | mwheel_up | triggerl_btn | — | press |
| Interaction Mode Zoom Out | `pc_zoom_out` | — | mwheel_down | triggerr_btn | — | press |
| MFD Left | `pc_screen_focus_left` | a | — | dpad_left | — | press |
| MFD Right | `pc_screen_focus_right` | d | — | dpad_right | — | press |
| MFD Up | `pc_screen_focus_up` | w | — | dpad_up | — | press |
| MFD Down | `pc_screen_focus_down` | s | — | dpad_down | — | press |
| Personal Inner Thought (PIT) | `pc_personal_thought` | lalt+f | — | back | — | press |
| Inventory Orbit Camera Mode | `pc_camera_orbit` | — | mouse2 | — | — | hold |
| Exit | `pc_personal_back` | np_0 | — | thumbr | — | press |
| pc_ui_back | `pc_ui_back` | — | mouse2 | — | — | press |
| Toggle Inventory (short press) | `pc_pit_inventory` | i | — | — | — | tap |
| Toogle Loot Screen (hold) | `pc_pit_looting` | i | — | — | — | delayed_press |
| Toggle Looting View | `pc_pit_looting_toggle_view` | tab | — | — | — | press |
| Looting - Toggle Weapon Attachments | `pc_pit_looting_toggle_weapon_attachments` | q | — | — | — | press |
| pc_pit_item_unstown | `pc_pit_item_unstown` | — | — | — | — | press |
| Drop Item | `pc_pit_item_drop` | — | — | — | — | press |
| Store All Commodities | `pc_pit_empty_backpack` | — | — | — | — | press |
| Player Actions - PIT Category | `pc_pit_player_actions` | — | — | — | — | press |
| Emotes - PIT Category | `pc_pit_emotes` | — | — | — | — | press |
| Ship Systems - PIT Category | `pc_pit_ship_systems` | — | — | — | — | press |
| Flight Systems - PIT Category | `pc_pit_flight_systems` | — | — | — | — | press |
| Vehicle Actions - PIT Category | `pc_pit_vehicle_actions` | — | — | — | — | press |
| Weapon Systems - PIT Category | `pc_pit_weapons_systems` | — | — | — | — | press |
| Remote Turret - PIT Category | `pc_pit_remote_turrets` | — | — | — | — | press |
| Item Actions - PIT Category | `pc_pit_item_actions` | — | — | — | — | press |
| Weapon Selection - PIT Category | `pc_pit_weapon_selection` | — | — | — | — | press |
| Mobiglas Actions - PIT Category | `pc_pit_mobiglas_actions` | — | — | — | — | press |
| Mining Mode Actions - PIT Category | `pc_pit_miningmode_actions` | lalt+m | — | — | — | press |
| Weapon Select Radial Menu | `pc_qs_weapons_pit_primary` | 1 | — | dpad_up | — | all |
| Weapon Select Radial Menu | `pc_qs_weapons_pit_secondary` | 2 | — | dpad_up | — | all |
| Weapon Select Radial Menu | `pc_qs_weapons_pit_sidearm` | 3 | — | dpad_up | — | all |
| Throwable Select Radial Menu | `pc_qs_grenades` | g | — | dpad_right | — | all |
| Consumable Select Radial Menu | `pc_qs_consumables` | 4 | — | dpad_left | — | all |

## flycam

`flycam` · 19 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| flycam_rotateyaw | `flycam_rotateyaw` | — | — | thumbrx | — | — |
| flycam_rotatepitch | `flycam_rotatepitch` | — | — | thumbry | — | — |
| flycam_rotateyaw_mouse | `flycam_rotateyaw_mouse` | — | maxis_x | — | — | — |
| flycam_rotatepitch_mouse | `flycam_rotatepitch_mouse` | — | maxis_y | — | — | — |
| flycam_movey | `flycam_movey` | — | — | thumbly | — | — |
| flycam_movefwd | `flycam_movefwd` | w | — | — | — | — |
| flycam_moveback | `flycam_moveback` | s | — | — | — | — |
| flycam_movex | `flycam_movex` | — | — | thumblx | — | — |
| flycam_moveright | `flycam_moveright` | d | — | — | — | — |
| flycam_moveleft | `flycam_moveleft` | a | — | — | — | — |
| flycam_movez | `flycam_movez` | — | — | shoulderl+thumbry | — | — |
| flycam_moveup | `flycam_moveup` | q | — | — | — | — |
| flycam_movedown | `flycam_movedown` | e | — | — | — | — |
| flycam_speedup | `flycam_speedup` | up | — | dpad_up | — | — |
| flycam_speeddown | `flycam_speeddown` | down | — | dpad_down | — | — |
| flycam_turbo | `flycam_turbo` | space | — | a | — | — |
| flycam_setpoint | `flycam_setpoint` | z | — | x | — | — |
| flycam_play | `flycam_play` | x | — | b | — | — |
| flycam_clear | `flycam_clear` | c | — | y | — | — |

## Camera - Advanced Camera Controls · CAMERA

`view_director_mode` · 32 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| Advanced Camera Controls Modifier (Hold) | `view_enable_camview_mode` | f4 | — | shoulderl+y | — | — |
| Advanced Camera Controls Modifier (Hold) | `view_switch_to_alternative` | z | — | — | — | — |
| Save View 1 | `view_save_view_1` | np_1 | — | — | — | delayed_press |
| Save View 2 | `view_save_view_2` | np_2 | — | — | — | delayed_press |
| Save View 3 | `view_save_view_3` | np_3 | — | — | — | delayed_press |
| Save View 4 | `view_save_view_4` | np_4 | — | — | — | delayed_press |
| Save View 5 | `view_save_view_5` | np_5 | — | — | — | delayed_press |
| Save View 6 | `view_save_view_6` | np_6 | — | — | — | delayed_press |
| Save View 7 | `view_save_view_7` | np_7 | — | — | — | delayed_press |
| Save View 8 | `view_save_view_8` | np_8 | — | — | — | delayed_press |
| Save View 9 | `view_save_view_9` | np_9 | — | — | — | delayed_press |
| Load View 1 | `view_load_view_1` | np_1 | — | — | — | tap |
| Load View 2 | `view_load_view_2` | np_2 | — | — | — | tap |
| Load View 3 | `view_load_view_3` | np_3 | — | — | — | tap |
| Load View 4 | `view_load_view_4` | np_4 | — | — | — | tap |
| Load View 5 | `view_load_view_5` | np_5 | — | — | — | tap |
| Load View 6 | `view_load_view_6` | np_6 | — | — | — | tap |
| Load View 7 | `view_load_view_7` | np_7 | — | — | — | tap |
| Load View 8 | `view_load_view_8` | np_8 | — | — | — | tap |
| Load View 9 | `view_load_view_9` | np_9 | — | — | — | tap |
| Clear Saved View | `view_reset_saved` | np_0 | — | — | — | delayed_press |
| X Offset Positive | `view_move_target_X_pos` | right | — | — | — | — |
| X Offset Negative | `view_move_target_X_neg` | left | — | — | — | — |
| Y Offset Positive / Spectator Freecam Focal Point Forward | `view_move_target_Y_pos` | up | — | — | — | — |
| Y Offset Negative / Spectator Freecam Focal Point Backward | `view_move_target_Y_neg` | down | — | — | — | — |
| Z Offset Positive | `view_move_target_Z_pos` | pgup | — | — | — | — |
| Z Offset Negative | `view_move_target_Z_neg` | pgdn | — | — | — | — |
| Increase FoV | `view_fov_in` | np_add | — | — | — | — |
| Decrease FoV | `view_fov_out` | np_subtract | — | — | — | — |
| Increase DoF | `view_fstop_in` | home | — | — | — | — |
| Decrease DoF | `view_fstop_out` | end | — | — | — | — |
| Reset Current View | `view_restore_defaults` | np_multiply | — | — | — | — |

## character_customizer

`character_customizer` · 28 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| character_customizer_yaw | `character_customizer_yaw` | — | maxis_x | — | — | — |
| character_customizer_pitch | `character_customizer_pitch` | — | maxis_y | — | — | — |
| character_customizer_gp_yaw | `character_customizer_gp_yaw` | — | — | thumblx | — | — |
| character_customizer_gp_pitch | `character_customizer_gp_pitch` | — | — | thumbly | — | — |
| character_customizer_zoom_in | `character_customizer_zoom_in` | — | mwheel_up | — | — | — |
| character_customizer_zoom_out | `character_customizer_zoom_out` | — | mwheel_down | — | — | — |
| character_customizer_select | `character_customizer_select` | f | mouse1 | a | — | — |
| character_customizer_enable_dna_edit | `character_customizer_enable_dna_edit` | — | mouse1 | — | — | — |
| character_customizer_enable_rotation | `character_customizer_enable_rotation` | lshift | — | — | — | hold |
| character_customizer_enable_mouse_rotation | `character_customizer_enable_mouse_rotation` | — | mouse2 | thumbr | — | hold |
| character_customizer_library_scroll_up | `character_customizer_library_scroll_up` | up | mwheel_up | — | — | — |
| character_customizer_library_scroll_down | `character_customizer_library_scroll_down` | down | mwheel_down | — | — | — |
| character_customizer_edit_dna_pos | `character_customizer_edit_dna_pos` | right | — | — | — | — |
| character_customizer_edit_dna_neg | `character_customizer_edit_dna_neg` | left | — | — | — | — |
| character_customizer_yaw_left | `character_customizer_yaw_left` | left | — | — | — | — |
| character_customizer_yaw_right | `character_customizer_yaw_right` | right | — | — | — | — |
| character_customizer_pitch_up | `character_customizer_pitch_up` | up | — | — | — | — |
| character_customizer_pitch_down | `character_customizer_pitch_down` | down | — | — | — | — |
| character_customizer_step_up | `character_customizer_step_up` | e | — | — | — | — |
| character_customizer_step_down | `character_customizer_step_down` | q | — | — | — | — |
| character_customizer_feature_up | `character_customizer_feature_up` | d | — | — | — | — |
| character_customizer_feature_down | `character_customizer_feature_down` | a | — | — | — | — |
| character_customizer_dnamode_up | `character_customizer_dnamode_up` | d | — | — | — | — |
| character_customizer_dnamode_down | `character_customizer_dnamode_down` | a | — | — | — | — |
| character_customizer_next_material_region | `character_customizer_next_material_region` | f | — | — | — | — |
| character_customizer_toggle_face_tracking | `character_customizer_toggle_face_tracking` | space | — | thumbl | — | — |
| character_customizer_dnaHandle_select | `character_customizer_dnaHandle_select` | — | — | a | — | — |
| character_customizer_dnaHandle_deselect | `character_customizer_dnaHandle_deselect` | — | — | b | — | — |

## RemoteRigidEntityController

`RemoteRigidEntityController` · 17 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| remote_moveForward | `remote_moveForward` | w | — | — | — | hold |
| remote_moveBack | `remote_moveBack` | s | — | — | — | hold |
| remote_moveLeft | `remote_moveLeft` | a | — | — | — | hold |
| remote_moveRight | `remote_moveRight` | d | — | — | — | hold |
| remote_moveUp | `remote_moveUp` | space | — | — | — | hold |
| remote_moveDown | `remote_moveDown` | lctrl | — | — | — | hold |
| remote_scaleUp | `remote_scaleUp` | c | — | — | — | hold |
| remote_scaleDown | `remote_scaleDown` | z | — | — | — | hold |
| remote_rollLeft | `remote_rollLeft` | q | — | — | — | hold |
| remote_rollRight | `remote_rollRight` | e | — | — | — | hold |
| remote_rotatePitch | `remote_rotatePitch` | — | maxis_x | — | — | — |
| remote_rotateYaw | `remote_rotateYaw` | — | maxis_y | — | — | — |
| remote_switchControl | `remote_switchControl` | v | — | — | — | tap |
| remote_stopControl | `remote_stopControl` | y | — | — | — | tap |
| remote_action1 | `remote_action1` | — | mouse1 | — | — | tap |
| remote_action2 | `remote_action2` | — | mouse2 | — | — | tap |
| remote_switchTarget | `remote_switchTarget` | b | — | — | — | tap |

## server_renderer

`server_renderer` · 1 actions

| Action | Id | Keyboard | Mouse | Gamepad | Joystick | Fires |
| --- | --- | --- | --- | --- | --- | --- |
| v_view_cycle_fwd | `v_view_cycle_fwd` | f4 | — | shoulderl+y | — | tap |

