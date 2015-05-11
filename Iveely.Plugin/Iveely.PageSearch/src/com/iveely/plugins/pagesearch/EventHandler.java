/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.plugins.pagesearch;

import com.iveely.framework.net.ICallback;
import com.iveely.framework.net.InternetPacket;
import com.iveely.framework.text.json.JsonUtil;
import com.iveely.plugins.pagesearch.data.Cache;
import java.io.UnsupportedEncodingException;
import java.util.ArrayList;
import java.util.List;

/**
 *
 * @author 凡平
 */
public class EventHandler implements ICallback {

    /**
     * Weather forecast.
     */
    private Cache cache;

    public EventHandler(Cache cache) {
        this.cache = cache;
    }

    @Override
    public InternetPacket invoke(InternetPacket packet) {
        InternetPacket respPacket = new InternetPacket();
        respPacket.setMimeType(0);
        respPacket.setExecutType(packet.getExecutType() * -1);
        if (packet.getExecutType() == 8) {
            try {
                String query = getString(packet.getData());
                System.out.println("文本搜索：" + query);
                String[] words = com.iveely.framework.segment.DicSegment.getInstance().split(query, true);
                List<Object[]> result = cache.search(query, words);
                respPacket.setData(getBytes(buildResult(result, words)));
            } catch (Exception e) {
                respPacket.setExecutType(Integer.MIN_VALUE);
                respPacket.setData(getBytes(e.toString()));
            }
            return respPacket;
        } else {
            return InternetPacket.getUnknowPacket();
        }
    }

    private String buildResult(List<Object[]> result, String[] keywords) {
        for (Object[] obj : result) {
            String simpleTitle = (obj[0].toString().length() > 28 ? obj[0].toString().substring(0, 27) + "..." : obj[0].toString());
            String colorTitle = changeColor(keywords, simpleTitle).replace("<!", "").trim();
            String colorContent = changeColor(keywords, checkContent(obj[2].toString().replace("  ", ""), keywords));
            obj[0] = colorTitle;
            obj[2] = colorContent;
        }
        return JsonUtil.listToJson(result);
    }

    /**
     * Change color of the text.
     *
     * @param keys
     * @param result
     * @return
     */
    private String changeColor(String[] keys, String result) {
        for (String key : keys) {
            if (!"".equals(key)) {
                result = result.replace(key, "<font color='#c00'>" + key + "</font>");
            }
        }
        return result;
    }

    /**
     * Check content length.
     *
     * @param content
     * @param keys
     * @return
     */
    public String checkContent(String content, String[] keys) {
        //Logger.debug(content);
        int abstractCount = 100;
        try {
            // 1. Determine the number of summary.
            if (content.length() < abstractCount) {
                return content;
            }

            // 2. Determine the number of blocks summary.
            int contentSize = content.length();
            int index = 0;
            List<Integer> list = new ArrayList<>();
            for (String key : keys) {
                index = content.indexOf(key, index);
                if (index < 0) {
                    continue;
                }
                list.add(index);
            }

            // 3. Up into three sections.
            if (list.size() == 1) {
                int place = list.get(0);
                if (contentSize - place < abstractCount) {
                    int start = place;
                    int end = place + 1;
                    while (end - start < abstractCount) {
                        if (start > 0) {
                            start--;
                        }
                        if (end < contentSize) {
                            end++;
                        }
                    }
                    return content.substring(start, end);
                } else if (place < abstractCount) {
                    return content.substring(0, abstractCount);
                } else {
                    int start = place - abstractCount / 2 - 1;
                    int end = place + abstractCount / 2;
                    if (end > contentSize - 1) {
                        end = contentSize - 1;
                    }
                    return content.substring(start, end);
                }
            } else if (list.size() > 1) {
                int placeA = list.get(0);
                int placeB = list.get(1);
                if (placeB - placeA < abstractCount) {
                    int start = placeA;
                    int end = placeB + 1;
                    while (end - start < abstractCount) {
                        if (start > 0) {
                            start--;
                        }
                        if (end < contentSize) {
                            end++;
                        }
                    }
                    if (end > contentSize - 1) {
                        end = contentSize - 1;
                    }
                    return content.substring(start, end);
                } else {
                    String bufferA = "";
                    if (placeA - abstractCount / 2 < 0) {
                        bufferA = content.substring(0, abstractCount / 2);
                    } else {
                        bufferA = content.substring(abstractCount / 2, abstractCount);
                    }
                    String bufferB = "";
                    if (contentSize - placeB < abstractCount / 2) {
                        int start = placeB + abstractCount / 2 - 1;
                        if (start > contentSize - 1) {
                            start = placeB;
                        }
                        bufferB = content.substring(start, contentSize - 1);
                    } else {
                        bufferB = content.substring(placeB - abstractCount / 4 - 1, placeB + abstractCount / 4);
                    }
                    return bufferA + bufferB;
                }
            } else {
                return content.substring(0, abstractCount) + "...";
            }
        } catch (Exception e) {
        }
        return content.substring(0, abstractCount) + "...";
    }

    /**
     * Convert string to byte[].
     *
     * @param content
     * @return
     */
    private byte[] getBytes(String content) {
        byte[] bytes;
        try {
            bytes = content.getBytes("UTF-8");
        } catch (UnsupportedEncodingException ex) {
            bytes = content.getBytes();
        }
        return bytes;
    }

    /**
     * Convert byte[] to string.
     *
     * @param bytes
     * @return
     */
    private String getString(byte[] bytes) {
        try {
            return new String(bytes, "UTF-8").trim();
        } catch (UnsupportedEncodingException ex) {
            return new String(bytes).trim();
        }
    }
}
