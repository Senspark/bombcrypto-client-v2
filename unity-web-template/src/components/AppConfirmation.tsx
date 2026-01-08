import { useEffect, useState } from 'react';
import { Modal } from 'antd';
import { useAtom } from 'jotai';
import { confirmationController } from '../controllers/ConfirmationService';
import ConfirmationService from '../controllers/ConfirmationService';

const confirmationService = new ConfirmationService();

export const AppConfirmation = () => {
    const [ctrl] = useAtom(confirmationController);
    const [isModalOpen, setIsModalOpen] = useState(false);

    useEffect(() => {
        if (!ctrl.isIdle) {
            setIsModalOpen(true);
        }
    }, [ctrl]);

    const handleOk = () => {
        setIsModalOpen(false);
        confirmationService.handleResponse(true);
    };

    const handleCancel = () => {
        setIsModalOpen(false);
        confirmationService.handleResponse(false);
    };

    return (
        <Modal
            title={ctrl.title}
            open={isModalOpen}
            onOk={handleOk}
            onCancel={handleCancel}
            okText="YES"
            cancelText="NO"
            maskClosable={false}
            closable={false}
        >
            <p>{ctrl.content}</p>
        </Modal>
    );
};