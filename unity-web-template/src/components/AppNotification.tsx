import {useEffect} from "react";
import {notification} from "antd";
import {
    IDepositNotification,
    INormalNotification,
    INotification,
    notificationController
} from "../controllers/NotificationService.ts";
import {useAtomValue} from "jotai";

export const AppNotification = () => {
    const [api, contextHolder] = notification.useNotification();
    const ctrl = useAtomValue(notificationController);

    const onNewNotification = (data: INotification) => {
        const deposit = data as IDepositNotification;
        if (deposit.txLink) {
            const link = deposit.txLink;
            api.open({
                message: 'Deposit Success',
                type: 'success',
                description: (
                    <>
                        This is your transaction tx:
                        <a href={link} target="_blank" rel="noopener noreferrer">{link}</a>
                    </>
                ),
            });
        }

        const normal = data as INormalNotification;
        if (normal.title) {
            api.open({
                message: normal.title,
                description: normal.description,
                duration: normal.duration,
                type: normal.type ?? 'info',
            });
        }
    };

    useEffect(() => {
        const clonedData = [...ctrl.data];
        ctrl.clear();
        // delay onNewNotification for each clonedData
        let i = 0;
        const timeStep = 500;
        clonedData.forEach(e => setTimeout(() => onNewNotification(e), (i++ * timeStep)));
    }, [ctrl]);

    return (
        <>
            {contextHolder}
        </>
    );
}