# Cross-Platform Push Notifications Implementation Plan

## Overview

This document outlines the comprehensive implementation plan for adding cross-platform push notifications to the Rivals game, enabling real-time notifications for iOS and Android Unity apps when server events occur.

## Architecture

### Technology Stack
- **Frontend**: Unity 2022.3+ (iOS & Android)
- **Backend**: Next.js 15.4.6 with PostgreSQL
- **Push Service**: Firebase Cloud Messaging (FCM)
- **Unity Packages**: Unity Mobile Notifications + Firebase Unity SDK
- **Server SDK**: Firebase Admin SDK

### Current App Configuration
- **iOS Bundle ID**: `nyc.rivals.app`
- **Android Package**: `com.unity.template.ar_mobile`
- **Unity Platform Support**: AR Foundation, iOS/Android modules

### Data Flow
```
Unity App → Register Device Token → Server API → PostgreSQL Storage
Game Event → Server Endpoint → Firebase Admin SDK → FCM → Device Notification
```

## Implementation Phases

### Phase 1: Firebase Project Setup

#### 1.1 Create Firebase Project
1. Navigate to [Firebase Console](https://console.firebase.google.com)
2. Create new project: "Rivals Game"
3. Enable Google Analytics (optional)

#### 1.2 Add Mobile Apps
**iOS Configuration:**
- Bundle ID: `nyc.rivals.app`
- Download `GoogleService-Info.plist`
- Place in `app/Assets/StreamingAssets/`

**Android Configuration:**
- Package name: `com.unity.template.ar_mobile`
- Download `google-services.json`
- Place in `app/Assets/StreamingAssets/`

#### 1.3 Enable Cloud Messaging
- Navigate to Project Settings → Cloud Messaging
- Generate and note Server Key (for server-side)
- Configure APNs certificates for iOS

### Phase 2: Database Schema Updates

#### 2.1 User Table Enhancement
```sql
-- Add device token and notification preferences to users table
ALTER TABLE users ADD COLUMN device_token VARCHAR(255);
ALTER TABLE users ADD COLUMN platform VARCHAR(10) CHECK (platform IN ('ios', 'android'));
ALTER TABLE users ADD COLUMN notifications_enabled BOOLEAN DEFAULT true;
ALTER TABLE users ADD COLUMN last_token_update TIMESTAMP DEFAULT CURRENT_TIMESTAMP;

-- Create index for efficient token lookups
CREATE INDEX idx_users_device_token ON users(device_token) WHERE device_token IS NOT NULL;
```

#### 2.2 Notifications Tracking Table
```sql
-- Table to track all sent notifications
CREATE TABLE notifications (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(id),
    title VARCHAR(255) NOT NULL,
    body TEXT NOT NULL,
    notification_type VARCHAR(50) NOT NULL,
    data JSONB,
    sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    delivered BOOLEAN DEFAULT false,
    read_at TIMESTAMP,
    fcm_message_id VARCHAR(255),
    
    -- Constraints
    CONSTRAINT valid_notification_type CHECK (
        notification_type IN ('trap_death', 'kill_achievement', 'proximity_alert', 'leaderboard_update', 'general')
    )
);

-- Indexes for performance
CREATE INDEX idx_notifications_user_id ON notifications(user_id);
CREATE INDEX idx_notifications_type ON notifications(notification_type);
CREATE INDEX idx_notifications_sent_at ON notifications(sent_at);
```

#### 2.3 Migration Scripts
```sql
-- Migration script to be run on existing database
-- File: server/pg_schema/migrations/001_add_push_notifications.sql

BEGIN;

-- Add new columns to users table
ALTER TABLE users 
ADD COLUMN IF NOT EXISTS device_token VARCHAR(255),
ADD COLUMN IF NOT EXISTS platform VARCHAR(10) CHECK (platform IN ('ios', 'android')),
ADD COLUMN IF NOT EXISTS notifications_enabled BOOLEAN DEFAULT true,
ADD COLUMN IF NOT EXISTS last_token_update TIMESTAMP DEFAULT CURRENT_TIMESTAMP;

-- Create notifications table
CREATE TABLE IF NOT EXISTS notifications (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(id),
    title VARCHAR(255) NOT NULL,
    body TEXT NOT NULL,
    notification_type VARCHAR(50) NOT NULL,
    data JSONB,
    sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    delivered BOOLEAN DEFAULT false,
    read_at TIMESTAMP,
    fcm_message_id VARCHAR(255),
    CONSTRAINT valid_notification_type CHECK (
        notification_type IN ('trap_death', 'kill_achievement', 'proximity_alert', 'leaderboard_update', 'general')
    )
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_users_device_token ON users(device_token) WHERE device_token IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_notifications_user_id ON notifications(user_id);
CREATE INDEX IF NOT EXISTS idx_notifications_type ON notifications(notification_type);
CREATE INDEX IF NOT EXISTS idx_notifications_sent_at ON notifications(sent_at);

COMMIT;
```

### Phase 3: Server Implementation

#### 3.1 Dependencies Installation
```bash
cd server
pnpm add firebase-admin
pnpm add @types/firebase-admin --save-dev
```

#### 3.2 Firebase Admin Configuration
**File: `server/app/lib/firebase-admin.ts`**
```typescript
import admin from 'firebase-admin';

if (!admin.apps.length) {
    const serviceAccount = {
        projectId: process.env.FIREBASE_PROJECT_ID,
        clientEmail: process.env.FIREBASE_CLIENT_EMAIL,
        privateKey: process.env.FIREBASE_PRIVATE_KEY?.replace(/\\n/g, '\n'),
    };

    admin.initializeApp({
        credential: admin.credential.cert(serviceAccount),
    });
}

export const messaging = admin.messaging();
export { admin };
```

#### 3.3 Notification Service Module
**File: `server/app/lib/notification-service.ts`**
```typescript
import { messaging } from './firebase-admin';
import { pg } from '@/app/pg';
import type { Pg } from '@/app/pg';

export interface NotificationData {
    userId: number;
    title: string;
    body: string;
    type: 'trap_death' | 'kill_achievement' | 'proximity_alert' | 'leaderboard_update' | 'general';
    data?: Record<string, string>;
}

export class NotificationService {
    private db: Pg;

    constructor() {
        this.db = pg();
    }

    async sendNotification(notificationData: NotificationData): Promise<boolean> {
        try {
            // Get user's device token
            const userResult = await this.db`
                SELECT device_token, platform, notifications_enabled 
                FROM users 
                WHERE id = ${notificationData.userId} 
                AND device_token IS NOT NULL 
                AND notifications_enabled = true
            `;

            if (userResult.length === 0) {
                console.log(`No valid device token for user ${notificationData.userId}`);
                return false;
            }

            const user = userResult[0];
            const message = {
                token: user.device_token,
                notification: {
                    title: notificationData.title,
                    body: notificationData.body,
                },
                data: {
                    type: notificationData.type,
                    ...notificationData.data,
                },
                android: {
                    notification: {
                        icon: 'ic_notification',
                        color: '#FF6B35',
                        sound: 'default',
                    },
                },
                apns: {
                    payload: {
                        aps: {
                            sound: 'default',
                            badge: 1,
                        },
                    },
                },
            };

            const response = await messaging.send(message);
            
            // Log notification to database
            await this.db`
                INSERT INTO notifications (user_id, title, body, notification_type, data, fcm_message_id)
                VALUES (${notificationData.userId}, ${notificationData.title}, ${notificationData.body}, 
                        ${notificationData.type}, ${JSON.stringify(notificationData.data)}, ${response})
            `;

            console.log('Notification sent successfully:', response);
            return true;
        } catch (error) {
            console.error('Error sending notification:', error);
            return false;
        }
    }

    async sendBulkNotifications(notifications: NotificationData[]): Promise<number> {
        let successCount = 0;
        
        for (const notification of notifications) {
            const success = await this.sendNotification(notification);
            if (success) successCount++;
        }
        
        return successCount;
    }
}
```

#### 3.4 API Endpoints

**File: `server/app/api/notifications/register-device/route.ts`**
```typescript
import { NextRequest, NextResponse } from "next/server";
import { z } from "zod";
import { pg } from "@/app/pg";
import { requireAuth, getOrCreateUser } from "@/app/lib/auth";

const schema = z.object({
    deviceToken: z.string().min(1),
    platform: z.enum(['ios', 'android']),
});

export async function POST(request: NextRequest) {
    try {
        const session = await requireAuth(request);
        const body = await request.json();
        const parsed = schema.safeParse(body);

        if (!parsed.success) {
            return NextResponse.json({ error: parsed.error.message }, { status: 400 });
        }

        const { deviceToken, platform } = parsed.data;
        const db = pg();
        const user = await getOrCreateUser(db, session.address);

        await db`
            UPDATE users 
            SET device_token = ${deviceToken}, 
                platform = ${platform}, 
                last_token_update = CURRENT_TIMESTAMP
            WHERE id = ${user.id}
        `;

        return NextResponse.json({ 
            message: "Device token registered successfully" 
        });
    } catch (error) {
        console.error("Device registration error:", error);
        return NextResponse.json({ error: "Internal server error" }, { status: 500 });
    }
}
```

**File: `server/app/api/notifications/send/route.ts`**
```typescript
import { NextRequest, NextResponse } from "next/server";
import { z } from "zod";
import { NotificationService } from "@/app/lib/notification-service";
import { requireAuth } from "@/app/lib/auth";

const schema = z.object({
    userId: z.number(),
    title: z.string(),
    body: z.string(),
    type: z.enum(['trap_death', 'kill_achievement', 'proximity_alert', 'leaderboard_update', 'general']),
    data: z.record(z.string()).optional(),
});

export async function POST(request: NextRequest) {
    try {
        await requireAuth(request); // Ensure authenticated
        
        const body = await request.json();
        const parsed = schema.safeParse(body);

        if (!parsed.success) {
            return NextResponse.json({ error: parsed.error.message }, { status: 400 });
        }

        const notificationService = new NotificationService();
        const success = await notificationService.sendNotification(parsed.data);

        if (success) {
            return NextResponse.json({ message: "Notification sent successfully" });
        } else {
            return NextResponse.json({ error: "Failed to send notification" }, { status: 500 });
        }
    } catch (error) {
        console.error("Send notification error:", error);
        return NextResponse.json({ error: "Internal server error" }, { status: 500 });
    }
}
```

### Phase 4: Unity Implementation

#### 4.1 Package Dependencies
Add to `app/Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.unity.mobile.notifications": "2.3.2",
    // ... existing dependencies
  }
}
```

#### 4.2 Firebase Unity SDK Integration
1. Download Firebase Unity SDK from [Firebase Unity Downloads](https://firebase.google.com/download/unity)
2. Import `FirebaseMessaging.unitypackage`
3. Place config files in `Assets/StreamingAssets/`

#### 4.3 Unity Scripts

**File: `app/Assets/Scripts/NotificationManager.cs`**
```csharp
using System;
using UnityEngine;
using Firebase;
using Firebase.Messaging;
using Unity.Notifications.Android;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    [Header("Notification Settings")]
    public string notificationChannelId = "rivals_notifications";
    public string notificationChannelName = "Rivals Game";
    public string notificationChannelDescription = "Notifications for Rivals game events";

    private AuthenticatedAPIClient apiClient;
    private bool firebaseInitialized = false;

    private void Start()
    {
        apiClient = FindObjectOfType<AuthenticatedAPIClient>();
        StartCoroutine(InitializeFirebase());
        SetupAndroidNotificationChannel();
    }

    private IEnumerator InitializeFirebase()
    {
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => dependencyTask.IsCompleted);

        if (dependencyTask.Result == DependencyStatus.Available)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            FirebaseMessaging.TokenReceived += OnTokenReceived;
            FirebaseMessaging.MessageReceived += OnMessageReceived;
            
            firebaseInitialized = true;
            Debug.Log("Firebase initialized successfully");
            
            // Request permission and get token
            RequestNotificationPermission();
        }
        else
        {
            Debug.LogError($"Could not resolve Firebase dependencies: {dependencyTask.Result}");
        }
    }

    private void SetupAndroidNotificationChannel()
    {
#if UNITY_ANDROID
        var channel = new AndroidNotificationChannel()
        {
            Id = notificationChannelId,
            Name = notificationChannelName,
            Importance = Importance.Default,
            Description = notificationChannelDescription,
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
    }

    private void RequestNotificationPermission()
    {
#if UNITY_IOS
        UnityEngine.iOS.NotificationServices.RegisterForNotifications(
            UnityEngine.iOS.NotificationType.Alert | 
            UnityEngine.iOS.NotificationType.Badge | 
            UnityEngine.iOS.NotificationType.Sound);
#endif
    }

    private void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log($"FCM Token received: {token.Token}");
        RegisterDeviceToken(token.Token);
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("Received a new message from: " + e.Message.From);
        Debug.Log("Message data: " + e.Message.Data);
        
        ProcessNotificationData(e.Message);
        ShowLocalNotification(e.Message);
    }

    private async void RegisterDeviceToken(string token)
    {
        if (apiClient == null) return;

        string platform = "";
#if UNITY_IOS
        platform = "ios";
#elif UNITY_ANDROID
        platform = "android";
#endif

        var requestData = new
        {
            deviceToken = token,
            platform = platform
        };

        try
        {
            await apiClient.PostAsync("/api/notifications/register-device", requestData);
            Debug.Log("Device token registered successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to register device token: {e.Message}");
        }
    }

    private void ProcessNotificationData(FirebaseMessage message)
    {
        if (message.Data.ContainsKey("type"))
        {
            string notificationType = message.Data["type"];
            
            switch (notificationType)
            {
                case "trap_death":
                    HandleTrapDeathNotification(message.Data);
                    break;
                case "kill_achievement":
                    HandleKillAchievementNotification(message.Data);
                    break;
                case "proximity_alert":
                    HandleProximityAlertNotification(message.Data);
                    break;
                case "leaderboard_update":
                    HandleLeaderboardUpdateNotification(message.Data);
                    break;
            }
        }
    }

    private void ShowLocalNotification(FirebaseMessage message)
    {
#if UNITY_ANDROID
        var notification = new AndroidNotification();
        notification.Title = message.Notification?.Title ?? "Rivals";
        notification.Text = message.Notification?.Body ?? "New game event";
        notification.FireTime = DateTime.Now;
        notification.SmallIcon = "icon_0";
        notification.LargeIcon = "icon_1";

        AndroidNotificationCenter.SendNotification(notification, notificationChannelId);
#elif UNITY_IOS
        var notification = new UnityEngine.iOS.LocalNotification();
        notification.alertTitle = message.Notification?.Title ?? "Rivals";
        notification.alertBody = message.Notification?.Body ?? "New game event";
        notification.fireDate = DateTime.Now;
        notification.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
        
        UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(notification);
#endif
    }

    private void HandleTrapDeathNotification(System.Collections.Generic.IDictionary<string, string> data)
    {
        // Handle trap death notification
        Debug.Log("Player died from trap!");
        // Update UI, show death screen, etc.
    }

    private void HandleKillAchievementNotification(System.Collections.Generic.IDictionary<string, string> data)
    {
        // Handle kill achievement notification  
        Debug.Log("Kill achievement unlocked!");
        // Show achievement UI, update stats, etc.
    }

    private void HandleProximityAlertNotification(System.Collections.Generic.IDictionary<string, string> data)
    {
        // Handle proximity alert
        Debug.Log("Trap detected nearby!");
        // Show warning UI, audio cue, etc.
    }

    private void HandleLeaderboardUpdateNotification(System.Collections.Generic.IDictionary<string, string> data)
    {
        // Handle leaderboard update
        Debug.Log("Leaderboard position changed!");
        // Update leaderboard UI, show rank change, etc.
    }

    private void OnDestroy()
    {
        if (firebaseInitialized)
        {
            FirebaseMessaging.TokenReceived -= OnTokenReceived;
            FirebaseMessaging.MessageReceived -= OnMessageReceived;
        }
    }
}
```

### Phase 5: Game Event Integration

#### 5.1 Update Existing Endpoints

**Modify `server/app/api/die/route.ts`:**
```typescript
// Add to imports
import { NotificationService } from "@/app/lib/notification-service";

// Add after successful transaction
if (receipt.status === "success") {
    // Send notification to trap owner if trap death
    if (data.trapId) {
        const notificationService = new NotificationService();
        await notificationService.sendNotification({
            userId: otherUserId,
            title: "💀 Trap Triggered!",
            body: `Your trap caught ${session.address}!`,
            type: 'trap_death',
            data: {
                victimAddress: session.address,
                trapId: data.trapId.toString()
            }
        });
    }
    
    return NextResponse.json({});
}
```

**Modify `server/app/api/kill-monster/route.ts`:**
```typescript
// Add to imports
import { NotificationService } from "@/app/lib/notification-service";

// Add after successful kill count update
const updatedUser = await db`
    SELECT kill_count FROM users WHERE id = ${user.id}
`;

const killCount = updatedUser[0].kill_count;

// Send achievement notification for milestone kills
if (killCount % 5 === 0) {
    const notificationService = new NotificationService();
    await notificationService.sendNotification({
        userId: user.id,
        title: "🎯 Kill Streak!",
        body: `Amazing! You've killed ${killCount} monsters!`,
        type: 'kill_achievement',
        data: {
            killCount: killCount.toString(),
            milestone: 'true'
        }
    });
}
```

### Phase 6: Environment Configuration

#### 6.1 Update Environment Templates

**Add to `server/env.local.template`:**
```bash
# Firebase Configuration for Push Notifications
FIREBASE_PROJECT_ID=your-firebase-project-id
FIREBASE_CLIENT_EMAIL=firebase-adminsdk-xxxxx@your-project.iam.gserviceaccount.com
FIREBASE_PRIVATE_KEY="-----BEGIN PRIVATE KEY-----\nYourPrivateKeyHere\n-----END PRIVATE KEY-----"

# Optional: FCM Server Key (legacy, use service account above)
FCM_SERVER_KEY=your-fcm-server-key
```

**Add to `server/env.local.multichain.template`:**
```bash
# Firebase Configuration for Push Notifications
FIREBASE_PROJECT_ID=your-firebase-project-id
FIREBASE_CLIENT_EMAIL=firebase-adminsdk-xxxxx@your-project.iam.gserviceaccount.com
FIREBASE_PRIVATE_KEY="-----BEGIN PRIVATE KEY-----\nYourPrivateKeyHere\n-----END PRIVATE KEY-----"
```

### Phase 7: Platform-Specific Setup

#### 7.1 iOS Configuration
1. **Xcode Project Setup:**
   - Add `GoogleService-Info.plist` to Xcode project
   - Enable Push Notifications capability
   - Configure APNs certificates in Firebase Console

2. **Unity iOS Settings:**
   - Player Settings → iOS → Other Settings → Target minimum iOS Version: 13.0+
   - Add required iOS frameworks in build settings

#### 7.2 Android Configuration
1. **Unity Android Settings:**
   - Player Settings → Android → Minimum API Level: 23+
   - Add `google-services.json` to `Assets/StreamingAssets/`

2. **Firebase Console:**
   - Upload APK to Firebase Console for testing
   - Configure FCM server key

### Phase 8: Testing Strategy

#### 8.1 Development Testing
1. **Local Testing:**
   - Use Firebase Console to send test messages
   - Test device token registration
   - Verify notification delivery

2. **Game Event Testing:**
   - Trigger kill monster events
   - Test trap placement and triggers
   - Verify notification content and timing

#### 8.2 Production Readiness
1. **Performance Testing:**
   - Test with multiple concurrent users
   - Monitor notification delivery rates
   - Check database performance under load

2. **Security Testing:**
   - Validate device token security
   - Test rate limiting
   - Verify user permission handling

## Notification Types & Templates

### 1. Trap Death Notifications
- **Title**: "💀 You were eliminated!"
- **Body**: "You were caught by [PlayerName]'s trap!"
- **Data**: `{ "trapId": "123", "killerAddress": "0x..." }`

### 2. Kill Achievement Notifications  
- **Title**: "🎯 Monster Slain!"
- **Body**: "Great shot! Kill streak: [count]"
- **Data**: `{ "killCount": "15", "milestone": "true" }`

### 3. Proximity Alert Notifications
- **Title**: "⚠️ Danger Detected!"
- **Body**: "Trap detected nearby - watch your step!"
- **Data**: `{ "trapDistance": "50m", "trapCount": "3" }`

### 4. Leaderboard Update Notifications
- **Title**: "👑 Rank Update!"
- **Body**: "You've moved up to rank #[position]!"
- **Data**: `{ "newRank": "5", "previousRank": "8" }`

## Security & Privacy Considerations

### Data Protection
- Device tokens are stored securely and associated with user accounts
- Notification content is minimal and non-sensitive
- Users can opt-out of notifications at any time

### Rate Limiting
- Implement notification rate limiting per user
- Prevent spam notifications from game events
- Monitor Firebase quota usage

### Permission Management
- Request notification permissions appropriately
- Respect user privacy settings
- Clear device tokens on app uninstall

## Monitoring & Analytics

### Key Metrics
- Notification delivery rates
- User engagement with notifications
- Device token registration success rates
- Firebase quota usage

### Logging Strategy
- Log all notification attempts in database
- Track delivery confirmations from Firebase
- Monitor error rates and failure reasons

## Future Enhancements

### Advanced Features
1. **Rich Notifications**: Images, actions, custom sounds
2. **Scheduled Notifications**: Daily login reminders, event alerts
3. **Geofencing**: Location-based proximity alerts
4. **Push Notification A/B Testing**: Optimize messaging effectiveness

### Optimization
1. **Batch Notifications**: Group similar notifications
2. **Smart Delivery**: Send at optimal times for each user
3. **Personalization**: Customize content based on player behavior

## Implementation Timeline

- **Week 1**: Firebase setup, database schema, server infrastructure
- **Week 2**: Unity integration, device registration, basic notifications
- **Week 3**: Game event integration, testing, debugging
- **Week 4**: Platform-specific setup, production deployment, monitoring

## Support & Maintenance

### Documentation
- API documentation for notification endpoints
- Unity integration guide for developers
- Troubleshooting guide for common issues

### Monitoring
- Set up alerts for notification failures
- Regular review of delivery metrics
- User feedback collection and analysis

---

This implementation plan provides a comprehensive roadmap for adding cross-platform push notifications to the Rivals game, ensuring reliable real-time communication between the server and mobile clients across iOS and Android platforms.
